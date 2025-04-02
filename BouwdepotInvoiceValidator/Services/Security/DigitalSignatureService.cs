using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using BouwdepotInvoiceValidator.Models;
using BouwdepotInvoiceValidator.Models.Enhanced; // Add this using statement
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BouwdepotInvoiceValidator.Services.Security
{
    /// <summary>
    /// Interface for digital signature service
    /// </summary>
    public interface IDigitalSignatureService
    {
        /// <summary>
        /// Signs a validation result to ensure integrity
        /// </summary>
        /// <param name="result">The validation result to sign</param>
        /// <returns>The signed validation result</returns>
        ValidationResult SignValidationResult(ValidationResult result);
        
        /// <summary>
        /// Verifies the signature of a validation result
        /// </summary>
        /// <param name="result">The validation result to verify</param>
        /// <returns>True if the signature is valid, false otherwise</returns>
        bool VerifySignature(ValidationResult result);
    }
    
    /// <summary>
    /// Configuration options for digital signature service
    /// </summary>
    public class DigitalSignatureOptions
    {
        /// <summary>
        /// Whether digital signatures are enabled
        /// </summary>
        public bool Enabled { get; set; } = true;
        
        /// <summary>
        /// Path to the certificate/key store
        /// </summary>
        public string KeyStorePath { get; set; } = "certificates/validation-keys";
        
        /// <summary>
        /// Key size in bits for RSA keys
        /// </summary>
        public int KeySize { get; set; } = 2048;
        
        /// <summary>
        /// Validity period in days for self-signed certificates
        /// </summary>
        public int CertificateValidityDays { get; set; } = 365;
        
        /// <summary>
        /// The signature algorithm to use (e.g., RSA-SHA256)
        /// </summary>
        public string SignatureAlgorithm { get; set; } = "RSA-SHA256";
    }
    
    /// <summary>
    /// Implementation of digital signature service
    /// </summary>
    public class DigitalSignatureService : IDigitalSignatureService
    {
        private readonly ILogger<DigitalSignatureService> _logger;
        private readonly DigitalSignatureOptions _options;
        private readonly RSA _privateKey;
        private readonly RSA _publicKey;
        
        /// <summary>
        /// Creates a new instance of the digital signature service
        /// </summary>
        public DigitalSignatureService(
            ILogger<DigitalSignatureService> logger,
            IConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // Load configuration options
            var securityConfig = configuration.GetSection("Security:DigitalSignature");
            _options = new DigitalSignatureOptions
            {
                Enabled = securityConfig.GetValue<bool>("Enabled", true),
                KeyStorePath = securityConfig.GetValue<string>("KeyStorePath", "certificates/validation-keys"),
                KeySize = securityConfig.GetValue<int>("KeySize", 2048),
                CertificateValidityDays = securityConfig.GetValue<int>("CertificateValidityDays", 365),
                SignatureAlgorithm = securityConfig.GetValue<string>("SignatureAlgorithm", "RSA-SHA256")
            };
            
            if (!_options.Enabled)
            {
                _logger.LogWarning("Digital signature service is disabled");
                return;
            }
            
            try
            {
                // Initialize the key store directory if it doesn't exist
                Directory.CreateDirectory(_options.KeyStorePath);
                
                // Check for existing keys
                string privateKeyPath = Path.Combine(_options.KeyStorePath, "private.xml");
                string publicKeyPath = Path.Combine(_options.KeyStorePath, "public.xml");
                
                if (File.Exists(privateKeyPath) && File.Exists(publicKeyPath))
                {
                    // Load existing keys
                    _privateKey = RSA.Create();
                    _privateKey.FromXmlString(File.ReadAllText(privateKeyPath));
                    
                    _publicKey = RSA.Create();
                    _publicKey.FromXmlString(File.ReadAllText(publicKeyPath));
                    
                    _logger.LogInformation("Loaded existing RSA keys from {KeyStore}", _options.KeyStorePath);
                }
                else
                {
                    // Generate new keys
                    _logger.LogInformation("Generating new RSA key pair in {KeyStore}", _options.KeyStorePath);
                    
                    _privateKey = RSA.Create(_options.KeySize);
                    _publicKey = RSA.Create();
                    
                    // Save the keys to files
                    File.WriteAllText(privateKeyPath, _privateKey.ToXmlString(includePrivateParameters: true));
                    File.WriteAllText(publicKeyPath, _privateKey.ToXmlString(includePrivateParameters: false));
                    
                    // Load the public key
                    _publicKey.FromXmlString(File.ReadAllText(publicKeyPath));
                    
                    _logger.LogInformation("Generated and saved new RSA key pair");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing digital signature service");
                throw;
            }
        }
        
        /// <inheritdoc />
        public ValidationResult SignValidationResult(ValidationResult result)
        {
            if (!_options.Enabled || result == null)
            {
                return result;
            }
            
            try
            {
                _logger.LogInformation("Signing validation result ID: {ValidationId}", result.ValidationId);
                
                // Create a clone of the result without the signature
                var clonedResult = new ValidationResult
                {
                    ValidationId = result.ValidationId,
                    IsValid = result.IsValid,
                    IsHomeImprovement = result.IsHomeImprovement,
                    IsBouwdepotCompliant = result.IsBouwdepotCompliant,
                    IsVerduurzamingsdepotCompliant = result.IsVerduurzamingsdepotCompliant,
                    ConfidenceScore = result.ConfidenceScore,
                    MeetsApprovalThreshold = result.MeetsApprovalThreshold,
                    ValidatedAt = result.ValidatedAt
                };
                
                // Serialize the result to JSON for signing
                string resultJson = JsonSerializer.Serialize(clonedResult, new JsonSerializerOptions
                {
                    WriteIndented = false,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                
                // Sign the JSON with our private key
                byte[] dataBytes = Encoding.UTF8.GetBytes(resultJson);
                byte[] signatureBytes;
                
                if (_options.SignatureAlgorithm.StartsWith("RSA-SHA256", StringComparison.OrdinalIgnoreCase))
                {
                    signatureBytes = _privateKey.SignData(dataBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                }
                else if (_options.SignatureAlgorithm.StartsWith("RSA-SHA512", StringComparison.OrdinalIgnoreCase))
                {
                    signatureBytes = _privateKey.SignData(dataBytes, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);
                }
                else
                {
                    signatureBytes = _privateKey.SignData(dataBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                }
                
                // Convert the signature to a Base64 string
                string signature = Convert.ToBase64String(signatureBytes);
                
                // Add the signature to the original result
                result.Signature = new DigitalSignature
                {
                    Algorithm = _options.SignatureAlgorithm,
                    SignatureValue = signature,
                    SignedFields = "ValidationId,IsValid,IsHomeImprovement,IsBouwdepotCompliant,IsVerduurzamingsdepotCompliant,ConfidenceScore,MeetsApprovalThreshold,ValidatedAt",
                    SignedAt = DateTime.UtcNow
                };
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error signing validation result");
                return result; // Return the result without a signature
            }
        }
        
        /// <inheritdoc />
        public bool VerifySignature(ValidationResult result)
        {
            if (!_options.Enabled || result == null || result.Signature == null)
            {
                return false;
            }
            
            try
            {
                _logger.LogInformation("Verifying signature for validation result ID: {ValidationId}", result.ValidationId);
                
                // Create a clone with only the signed fields
                var clonedResult = new ValidationResult
                {
                    ValidationId = result.ValidationId,
                    IsValid = result.IsValid,
                    IsHomeImprovement = result.IsHomeImprovement,
                    IsBouwdepotCompliant = result.IsBouwdepotCompliant,
                    IsVerduurzamingsdepotCompliant = result.IsVerduurzamingsdepotCompliant,
                    ConfidenceScore = result.ConfidenceScore,
                    MeetsApprovalThreshold = result.MeetsApprovalThreshold,
                    ValidatedAt = result.ValidatedAt
                };
                
                // Serialize the result to JSON
                string resultJson = JsonSerializer.Serialize(clonedResult, new JsonSerializerOptions
                {
                    WriteIndented = false,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                
                // Verify the signature
                byte[] dataBytes = Encoding.UTF8.GetBytes(resultJson);
                byte[] signatureBytes = Convert.FromBase64String(result.Signature.SignatureValue);
                
                bool isValid;
                if (result.Signature.Algorithm.StartsWith("RSA-SHA256", StringComparison.OrdinalIgnoreCase))
                {
                    isValid = _publicKey.VerifyData(dataBytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                }
                else if (result.Signature.Algorithm.StartsWith("RSA-SHA512", StringComparison.OrdinalIgnoreCase))
                {
                    isValid = _publicKey.VerifyData(dataBytes, signatureBytes, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);
                }
                else
                {
                    isValid = _publicKey.VerifyData(dataBytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                }
                
                if (isValid)
                {
                    _logger.LogInformation("Signature verification successful for validation result ID: {ValidationId}", result.ValidationId);
                }
                else
                {
                    _logger.LogWarning("Signature verification failed for validation result ID: {ValidationId}", result.ValidationId);
                }
                
                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying signature for validation result ID: {ValidationId}", result.ValidationId);
                return false;
            }
        }
    }
}
