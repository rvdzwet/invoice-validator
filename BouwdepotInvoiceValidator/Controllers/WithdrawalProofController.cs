using BouwdepotInvoiceValidator.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace BouwdepotInvoiceValidator.Controllers
{
    /// <summary>
    /// Controller for withdrawal proof validation operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class WithdrawalProofController : ControllerBase
    {
        private readonly ILogger<WithdrawalProofController> _logger;
        private readonly IWithdrawalProofValidationService _validationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="WithdrawalProofController"/> class
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="validationService">The withdrawal proof validation service</param>
        public WithdrawalProofController(
            ILogger<WithdrawalProofController> logger,
            IWithdrawalProofValidationService validationService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
        }

        /// <summary>
        /// Validates a withdrawal proof document (PDF only) and returns comprehensive validation results
        /// </summary>
        /// <param name="file">The document file to validate (must be a PDF)</param>
        /// <returns>The validation result</returns>
        [HttpPost("validate")]
        [ProducesResponseType(typeof(ValidationContext), 200)] // Assuming ValidationContext is the correct return type based on the original code
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        [ProducesResponseType(typeof(ProblemDetails), 415)] // Added for unsupported media type
        [ProducesResponseType(typeof(ProblemDetails), 500)]
        public async Task<IActionResult> ValidateWithdrawalProof(
            IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("No file provided or file is empty for validation request.");
                return BadRequest("No file provided or file is empty");
            }

            try
            {
                _logger.LogInformation("Processing withdrawal proof validation request for file {FileName}", file.FileName);

                // Create a stream from the uploaded file
                using var fileStream = file.OpenReadStream();

                // Validate the document
                // Note: The original code passes file.ContentType to the service.
                // Since we now only accept PDF, you might adjust the service
                // if it previously handled multiple types based on ContentType.
                var validationContext = await _validationService.ValidateWithdrawalProofAsync(
                    fileStream,
                    file.FileName,
                    file.ContentType); // ContentType will now always be "application/pdf"

                // Return the ComprehensiveValidationResult directly from the context
                if (validationContext != null)
                {
                    _logger.LogInformation("Withdrawal proof validation completed successfully for file {FileName}", file.FileName);
                    return Ok(validationContext);
                }
                else
                {
                    _logger.LogWarning("ValidationContext was null for file {FileName}", file.FileName);
                    return StatusCode(500, new ProblemDetails
                    {
                        Title = "Validation result not available",
                        Detail = "The validation process completed but did not produce a result.",
                        Status = 500
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating withdrawal proof file {FileName}", file.FileName);
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Error processing withdrawal proof",
                    Detail = "An unexpected error occurred during validation",
                    Status = 500
                });
            }
        }
    }

    /// <summary>
    /// DTO for a validation issue
    /// </summary>
    public class ValidationIssueDto
    {
        /// <summary>
        /// Issue code
        /// </summary>
        public string Code { get; set; }
        
        /// <summary>
        /// Issue message
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// Issue severity
        /// </summary>
        public string Severity { get; set; }
    }

    /// <summary>
    /// Response model for withdrawal proof validation
    /// </summary>
    public class WithdrawalProofValidationResponse
    {
        /// <summary>
        /// Unique identifier for this validation
        /// </summary>
        public string ValidationId { get; set; }
        
        /// <summary>
        /// Validation status
        /// </summary>
        public string Status { get; set; }
        
        /// <summary>
        /// Summary of the validation result
        /// </summary>
        public string Summary { get; set; }
        
        /// <summary>
        /// List of validation issues
        /// </summary>
        public List<ValidationIssueDto> Issues { get; set; } = new List<ValidationIssueDto>();
    }
}
