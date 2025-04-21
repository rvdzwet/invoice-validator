using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouwdepotInvoiceValidator.Domain.Services.Services
{
    internal class GhostscriptOptions
    {
        public const string Ghostscript = "Ghostscript"; // Section name in configuration

        public string ExecutablePath { get; set; } = string.Empty;
    }

    // Class implementing the IPdfToImageConverter using Ghostscript
    internal class GhostscriptPdfToImageConverter : IPdfToImageConverter
    {
        private readonly string _ghostscriptPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="GhostscriptPdfToImageConverter"/> class.
        /// </summary>
        /// <param name="ghostscriptOptions">The configured Ghostscript options.</param>
        /// <exception cref="ArgumentNullException">Thrown if ghostscriptOptions is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the ExecutablePath in options is null or empty.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the Ghostscript executable is not found at the specified path.</exception>
        public GhostscriptPdfToImageConverter(IOptions<GhostscriptOptions> ghostscriptOptions)
        {
            if (ghostscriptOptions == null)
            {
                throw new ArgumentNullException(nameof(ghostscriptOptions));
            }

            var options = ghostscriptOptions.Value;

            if (string.IsNullOrEmpty(options.ExecutablePath))
            {
                throw new ArgumentException("Ghostscript executable path cannot be null or empty in the configuration.", nameof(options.ExecutablePath));
            }

            if (!File.Exists(options.ExecutablePath))
            {
                throw new FileNotFoundException($"Ghostscript executable not found at: {options.ExecutablePath}", options.ExecutablePath);
            }

            _ghostscriptPath = options.ExecutablePath;
        }

        /// <inheritdoc />
        public async Task ConvertPdfPageToImageAsync(Stream pdfStream, string outputImagePath, int pageNumber, int resolution, string imageFormat)
        {
            if (pdfStream == null)
            {
                throw new ArgumentNullException(nameof(pdfStream), "Input PDF stream cannot be null.");
            }

            if (string.IsNullOrEmpty(outputImagePath))
            {
                throw new ArgumentNullException(nameof(outputImagePath), "Output image path cannot be null or empty.");
            }

            if (pageNumber <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be greater than zero.");
            }

            if (resolution <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(resolution), "Resolution must be greater than zero.");
            }

            if (string.IsNullOrEmpty(imageFormat))
            {
                throw new ArgumentNullException(nameof(imageFormat), "Image format cannot be null or empty.");
            }

            // Create a temporary file to save the PDF stream
            string tempPdfFile = Path.GetTempFileName() + ".pdf";
            try
            {
                using (var fileStream = new FileStream(tempPdfFile, FileMode.Create, FileAccess.Write))
                {
                    await pdfStream.CopyToAsync(fileStream);
                }

                // Construct the Ghostscript command
                string arguments = $"-sDEVICE={imageFormat} " +
                                   $"-dJPEGQ=100 " + // For JPEG quality (0-100)
                                   $"-r{resolution} " +
                                   $"-g<image width>x<image height> " + // Optional: Specify image dimensions
                                   $"-o\"{outputImagePath}\" " +
                                   $"-dFirstPage={pageNumber} " +
                                   $"-dLastPage={pageNumber} " +
                                   $"\"{tempPdfFile}\"";

                using (var process = new Process())
                {
                    process.StartInfo.FileName = _ghostscriptPath;
                    process.StartInfo.Arguments = arguments;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.CreateNoWindow = true;

                    process.Start();
                    string errorOutput = await process.StandardError.ReadToEndAsync();
                    await process.WaitForExitAsync();

                    if (process.ExitCode != 0)
                    {
                        throw new Exception($"Ghostscript conversion failed. Exit code: {process.ExitCode}. Error: {errorOutput}");
                    }
                }
            }
            finally
            {
                // Clean up the temporary PDF file
                if (File.Exists(tempPdfFile))
                {
                    File.Delete(tempPdfFile);
                }
            }
        }
    }

    // Interface defining the PDF to Image conversion contract
    internal interface IPdfToImageConverter
    {
        /// <summary>
        /// Converts a specific page of a PDF file (provided as a FileStream) to an image file.
        /// </summary>
        /// <param name="pdfStream">The input PDF file stream.</param>
        /// <param name="outputImagePath">The path where the output image should be saved.</param>
        /// <param name="pageNumber">The page number to convert (1-based index).</param>
        /// <param name="resolution">The desired resolution of the output image (in DPI).</param>
        /// <param name="imageFormat">The desired image format (e.g., "png", "jpeg", "tiff").</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown if pdfStream or outputImagePath is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if pageNumber or resolution is less than or equal to zero.</exception>
        /// <exception cref="FileNotFoundException">Thrown if Ghostscript executable is not found.</exception>
        /// <exception cref="Exception">Thrown for other conversion errors.</exception>
        Task ConvertPdfPageToImageAsync(Stream pdfStream, string outputImagePath, int pageNumber, int resolution, string imageFormat);
    }
}
