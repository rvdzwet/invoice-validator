using BouwdepotInvoiceValidator.Domain.Models;
using BouwdepotInvoiceValidator.Domain.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace BouwdepotInvoiceValidator.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableCors("AllowAll")]
    public class InvoiceController : ControllerBase
    {
        private readonly IInvoiceValidationService _validationService;
        private readonly ILogger<InvoiceController> _logger;

        public InvoiceController(
            IInvoiceValidationService validationService,
            ILogger<InvoiceController> logger)
        {
            _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Validates an invoice PDF file
        /// </summary>
        /// <param name="file">The PDF file to validate</param>
        /// <returns>Validation results including extracted data and validation issues, and an audit report</returns>
        [HttpPost("validate")]
        [ProducesResponseType(typeof(ValidationResult), StatusCodes.Status200OK)] // Updated return type to object for combined result
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

                var result = await _validationService.ValidateInvoiceAsync(stream, file.FileName, file.ContentType);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating invoice");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { error = "An error occurred while processing the invoice", message = ex.Message });
            }
        }
    }
}