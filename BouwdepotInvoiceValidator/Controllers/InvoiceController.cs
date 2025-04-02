using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BouwdepotInvoiceValidator.Models;
using BouwdepotInvoiceValidator.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BouwdepotInvoiceValidator.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableCors("AllowAll")]
    public class InvoiceController : ControllerBase
    {
        private readonly IInvoiceValidationService _validationService;
        private readonly IAuditReportService _auditReportService;
        private readonly IGeminiService _geminiService;
        private readonly ILogger<InvoiceController> _logger;

        public InvoiceController(
            IInvoiceValidationService validationService,
            IAuditReportService auditReportService,
            IGeminiService geminiService,
            ILogger<InvoiceController> logger)
        {
            _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
            _auditReportService = auditReportService ?? throw new ArgumentNullException(nameof(auditReportService));
            _geminiService = geminiService ?? throw new ArgumentNullException(nameof(geminiService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Validates an invoice PDF file
        /// </summary>
        /// <param name="file">The PDF file to validate</param>
        /// <returns>Validation results including extracted data and validation issues</returns>
        [HttpPost("validate")]
        [ProducesResponseType(typeof(ValidationResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [RequestFormLimits(MultipartBodyLengthLimit = 10 * 1024 * 1024)] // 10MB limit
        public async Task<IActionResult> ValidateInvoice(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    _logger.LogWarning("No file was uploaded");
                    return BadRequest("No file was uploaded");
                }

                if (file.ContentType != "application/pdf" && !file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Invalid file type. Only PDF files are accepted");
                    return BadRequest("Invalid file type. Only PDF files are accepted");
                }

                _logger.LogInformation("Processing invoice validation request for file: {FileName}, Size: {FileSize} bytes", 
                    file.FileName, file.Length);

                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                stream.Position = 0;

                var result = await _validationService.ValidateInvoiceAsync(stream, file.FileName);
                
                _logger.LogInformation("Invoice validation completed. IsValid: {IsValid}, IsHomeImprovement: {IsHomeImprovement}, " +
                    "PossibleTampering: {PossibleTampering}, IsVerifiedInvoice: {IsVerifiedInvoice}, " +
                    "DetectedDocumentType: {DetectedDocumentType}, Issues: {IssueCount}", 
                    result.IsValid, result.IsHomeImprovement, result.PossibleTampering, 
                    result.IsVerifiedInvoice, result.DetectedDocumentType, result.Issues.Count);

                // Automatically generate audit report
                _logger.LogInformation("Automatically generating audit report for validation ID: {ValidationId}", result.ValidationId);
                var auditReport = await _auditReportService.GenerateAuditReportAsync(result);
                
                // Create a combined result object
                var combinedResult = new 
                {
                    ValidationResult = result,
                    AuditReport = auditReport
                };

                return Ok(combinedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating invoice");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { error = "An error occurred while processing the invoice", message = ex.Message });
            }
        }

        /// <summary>
        /// Generates a consolidated audit report for a validation result
        /// </summary>
        /// <param name="validationId">The ID of the validation result</param>
        /// <returns>A consolidated audit report</returns>
        [HttpGet("audit-report/{validationId}")]
        [ProducesResponseType(typeof(ConsolidatedAuditReport), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAuditReport(string validationId)
        {
            try
            {
                _logger.LogInformation("Generating audit report for validation ID: {ValidationId}", validationId);
                
                // Get the validation result 
                var validationResult = await _validationService.GetValidationResultAsync(validationId);
                if (validationResult == null)
                {
                    _logger.LogWarning("Validation result not found for ID: {ValidationId}", validationId);
                    return NotFound($"No validation result found with ID: {validationId}");
                }
                
                // Generate the audit report
                var auditReport = await _auditReportService.GenerateAuditReportAsync(validationResult);
                
                _logger.LogInformation("Audit report generated successfully for validation ID: {ValidationId}", validationId);
                return Ok(auditReport);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating audit report for validation ID: {ValidationId}", validationId);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { error = "An error occurred while generating the audit report", message = ex.Message });
            }
        }
        
        /// <summary>
        /// Process additional pages of a PDF for more comprehensive analysis
        /// </summary>
        /// <param name="validationId">The ID of the validation result</param>
        /// <param name="startPage">The page number to start processing from (optional, defaults to 2)</param>
        /// <returns>Success status and the number of additional pages processed</returns>
        [HttpPost("process-additional-pages/{validationId}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ProcessAdditionalPages(string validationId, [FromQuery] int startPage = 2)
        {
            try
            {
                _logger.LogInformation("Processing additional pages for validation ID: {ValidationId} starting from page {StartPage}", 
                    validationId, startPage);
                
                // Get the validation result 
                var validationResult = await _validationService.GetValidationResultAsync(validationId);
                if (validationResult == null || validationResult.ExtractedInvoice == null)
                {
                    _logger.LogWarning("Validation result not found for ID: {ValidationId}", validationId);
                    return NotFound($"No validation result found with ID: {validationId}");
                }
                
                // Process additional pages
                var pagesProcessed = await _validationService.ProcessAdditionalPagesAsync(validationId, startPage);
                
                _logger.LogInformation("Successfully processed {PageCount} additional pages for validation ID: {ValidationId}", 
                    pagesProcessed, validationId);
                
                return Ok(new 
                { 
                    success = true, 
                    message = $"Successfully processed {pagesProcessed} additional pages",
                    pagesProcessed = pagesProcessed 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing additional pages for validation ID: {ValidationId}", validationId);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { error = "An error occurred while processing additional pages", message = ex.Message });
            }
        }

        /// <summary>
        /// Start a new conversation with Gemini
        /// </summary>
        /// <param name="request">Optional metadata for the conversation</param>
        /// <returns>The new conversation details</returns>
        [HttpPost("conversation/start")]
        [ProducesResponseType(typeof(ConversationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult StartConversation([FromBody] StartConversationRequest request)
        {
            try
            {
                _logger.LogInformation("Starting new conversation");
                
                Dictionary<string, string> metadata = null;
                if (request?.Metadata != null)
                {
                    metadata = request.Metadata;
                }
                
                var conversationId = _geminiService.StartNewConversation(metadata);
                var conversation = _geminiService.GetCurrentConversation();
                
                _logger.LogInformation("Started new conversation with ID: {ConversationId}", conversationId);
                
                return Ok(new ConversationResponse
                {
                    ConversationId = conversationId,
                    Status = "active",
                    MessageCount = conversation.Messages.Count,
                    CreatedAt = conversation.CreatedAt,
                    LastUpdatedAt = conversation.LastUpdatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting new conversation");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { error = "An error occurred while starting a new conversation", message = ex.Message });
            }
        }
        
        /// <summary>
        /// Get information about the current conversation
        /// </summary>
        /// <returns>Information about the current conversation</returns>
        [HttpGet("conversation/current")]
        [ProducesResponseType(typeof(ConversationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetCurrentConversation()
        {
            try
            {
                _logger.LogInformation("Getting information about current conversation");
                
                var conversation = _geminiService.GetCurrentConversation();
                
                return Ok(new ConversationResponse
                {
                    ConversationId = conversation.ConversationId,
                    Status = "active",
                    MessageCount = conversation.Messages.Count,
                    CreatedAt = conversation.CreatedAt,
                    LastUpdatedAt = conversation.LastUpdatedAt,
                    Metadata = conversation.Metadata
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current conversation information");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { error = "An error occurred while getting conversation information", message = ex.Message });
            }
        }
        
        /// <summary>
        /// Switch to an existing conversation
        /// </summary>
        /// <param name="conversationId">ID of the conversation to switch to</param>
        /// <returns>Information about the switched conversation</returns>
        [HttpPost("conversation/switch/{conversationId}")]
        [ProducesResponseType(typeof(ConversationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult SwitchConversation(string conversationId)
        {
            try
            {
                _logger.LogInformation("Switching to conversation: {ConversationId}", conversationId);
                
                bool success = _geminiService.SwitchConversation(conversationId);
                if (!success)
                {
                    _logger.LogWarning("Conversation not found or timed out: {ConversationId}", conversationId);
                    return NotFound($"Conversation with ID {conversationId} not found or has timed out");
                }
                
                var conversation = _geminiService.GetCurrentConversation();
                
                return Ok(new ConversationResponse
                {
                    ConversationId = conversation.ConversationId,
                    Status = "active",
                    MessageCount = conversation.Messages.Count,
                    CreatedAt = conversation.CreatedAt,
                    LastUpdatedAt = conversation.LastUpdatedAt,
                    Metadata = conversation.Metadata
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error switching conversation");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { error = "An error occurred while switching conversation", message = ex.Message });
            }
        }
        
        /// <summary>
        /// Clear the message history of the current conversation
        /// </summary>
        /// <returns>Information about the cleared conversation</returns>
        [HttpPost("conversation/clear")]
        [ProducesResponseType(typeof(ConversationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult ClearConversation()
        {
            try
            {
                _logger.LogInformation("Clearing current conversation history");
                
                _geminiService.ClearCurrentConversation();
                var conversation = _geminiService.GetCurrentConversation();
                
                return Ok(new ConversationResponse
                {
                    ConversationId = conversation.ConversationId,
                    Status = "active",
                    MessageCount = conversation.Messages.Count,
                    CreatedAt = conversation.CreatedAt,
                    LastUpdatedAt = conversation.LastUpdatedAt,
                    Metadata = conversation.Metadata
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing conversation history");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { error = "An error occurred while clearing conversation history", message = ex.Message });
            }
        }
        
        /// <summary>
        /// Send a prompt to Gemini and get a response
        /// </summary>
        /// <param name="request">The prompt request</param>
        /// <returns>Gemini's response</returns>
        [HttpPost("conversation/prompt")]
        [ProducesResponseType(typeof(PromptResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SendPrompt([FromBody] PromptRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.Prompt))
                {
                    return BadRequest("Prompt cannot be empty");
                }
                
                _logger.LogInformation("Sending prompt to Gemini: {Prompt}", request.TruncatedPrompt);
                
                bool useHistory = request.UseHistory ?? true;
                
                var response = await _geminiService.GetConversationPromptAsync(request.Prompt, useHistory);
                var conversation = _geminiService.GetCurrentConversation();
                
                return Ok(new PromptResponse
                {
                    Response = response,
                    ConversationId = conversation.ConversationId,
                    MessageCount = conversation.Messages.Count,
                    LastUpdatedAt = conversation.LastUpdatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending prompt to Gemini");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { error = "An error occurred while sending prompt to Gemini", message = ex.Message });
            }
        }
        
        /// <summary>
        /// Health check endpoint
        /// </summary>
        /// <returns>API status</returns>
        [HttpGet("health")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public IActionResult HealthCheck()
        {
            return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
        }
        
        #region Request and Response Types
        
        public class StartConversationRequest
        {
            public Dictionary<string, string> Metadata { get; set; }
        }
        
        public class ConversationResponse
        {
            public string ConversationId { get; set; }
            public string Status { get; set; }
            public int MessageCount { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime LastUpdatedAt { get; set; }
            public Dictionary<string, string> Metadata { get; set; }
        }
        
        public class PromptRequest
        {
            public string Prompt { get; set; }
            public bool? UseHistory { get; set; }
            
            public string TruncatedPrompt => Prompt?.Length > 50 
                ? Prompt.Substring(0, 47) + "..." 
                : Prompt;
        }
        
        public class PromptResponse
        {
            public string Response { get; set; }
            public string ConversationId { get; set; }
            public int MessageCount { get; set; }
            public DateTime LastUpdatedAt { get; set; }
        }
        
        #endregion
    }
}
