# Secure Message Service

## Service

```jsx
using System;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace SecureMessagingApp.Services
{
    public class SecureMessageService
    {
        private readonly ICryptoService _cryptoService;
        private readonly ILogger<SecureMessageService> _logger;

        public SecureMessageService(ICryptoService cryptoService, ILogger<SecureMessageService> logger)
        {
            _cryptoService = cryptoService;
            _logger = logger;
        }

        public (byte[] PublicKey, byte[] PrivateKey) RegisterUser()
        {
            try
            {
                // Generate a master key pair for the user
                var (publicKey, privateKey) = _cryptoService.GenerateMasterKeyPair();
                _logger.LogInformation("User registered with master public key: {PublicKey}", Convert.ToBase64String(publicKey));
                return (publicKey, privateKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user.");
                throw;
            }
        }

        public (byte[] PublicKey, byte[] PrivateKey) GenerateChildKey(byte[] masterPrivateKey, string path)
        {
            try
            {
                // Derive a child key pair from the master key pair
                var (publicKey, privateKey) = _cryptoService.DeriveChildKeyPair(masterPrivateKey, path);
                _logger.LogInformation("Derived child key pair from path {Path} with public key: {PublicKey}", path, Convert.ToBase64String(publicKey));
                return (publicKey, privateKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating child key pair.");
                throw;
            }
        }

        public byte[] EncryptMessage(byte[] recipientPublicKey, string message)
        {
            try
            {
                // Encrypt the message using the recipient's public key
                var encryptedMessage = _cryptoService.EncryptData(recipientPublicKey, message);
                _logger.LogInformation("Message encrypted.");
                return encryptedMessage;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error encrypting message.");
                throw;
            }
        }

        public string DecryptMessage(byte[] privateKey, byte[] encryptedMessage)
        {
            try
            {
                // Decrypt the message using the recipient's private key
                var message = _cryptoService.DecryptData(privateKey, encryptedMessage);
                _logger.LogInformation("Message decrypted.");
                return message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error decrypting message.");
                throw;
            }
        }

        public byte[] SignMessage(byte[] privateKey, string message)
        {
            try
            {
                // Sign the message with the user's private key
                var signature = _cryptoService.SignData(privateKey, message);
                _logger.LogInformation("Message signed.");
                return signature;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error signing message.");
                throw;
            }
        }

        public bool VerifyMessage(byte[] publicKey, string message, byte[] signature)
        {
            try
            {
                // Verify the message signature with the user's public key
                var isVerified = _cryptoService.VerifyData(publicKey, message, signature);
                _logger.LogInformation("Message signature verified: {IsVerified}", isVerified);
                return isVerified;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying message signature.");
                throw;
            }
        }

        public bool VerifyMessageSignatureFromDerivedKey(byte[] masterPublicKey, string interimPath, string message, byte[] signature)
        {
            try
            {
                // Derive the interim public key
                var interimKey = _cryptoService.DeriveChildKeyPair(masterPublicKey, interimPath);
                
                // Verify the signature with the interim public key
                var isVerified = _cryptoService.VerifyData(interimKey.PublicKey, message, signature);
                
                // Additionally, derive the child key directly from the master public key to ensure consistency
                var childKeyFromMaster = _cryptoService.DeriveChildKeyPair(masterPublicKey, $"{interimPath}");
                var isChildVerified = _cryptoService.VerifyData(childKeyFromMaster.PublicKey, message, signature);

                var result = isVerified && isChildVerified;
                _logger.LogInformation("Message signature verified with derived key: {IsVerified}", result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying message signature with derived key.");
                throw;
            }
        }
    }
}
```