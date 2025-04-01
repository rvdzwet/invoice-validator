using System;
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
    [EnableCors("AllowReactApp")]
    public class InvoiceController : ControllerBase
    {
        private readonly IInvoiceValidationService _validationService;
        private readonly ILogger<InvoiceController> _logger;

        public InvoiceController(IInvoiceValidationService validationService, ILogger<InvoiceController> logger)
        {
            _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
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
                    "PossibleTampering: {PossibleTampering}, Issues: {IssueCount}", 
                    result.IsValid, result.IsHomeImprovement, result.PossibleTampering, result.Issues.Count);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating invoice");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { error = "An error occurred while processing the invoice", message = ex.Message });
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
    }
}
