# Unit Test for Key Rotation

```csharp
using System;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using SecureMessagingApp.Services;
using SecureMessagingApp.Models;

namespace SecureMessagingApp.Tests
{
    public class SecureMessageServiceTests
    {
        private readonly Mock<ILogger<CryptoService>> _loggerMock;
        private readonly ICryptoService _cryptoService;

        public SecureMessageServiceTests()
        {
            _loggerMock = new Mock<ILogger<CryptoService>>();
            _cryptoService = new CryptoService(_loggerMock.Object);
        }

        [Fact]
        public void GenerateAndRecoverMasterKeyFromMnemonic()
        {
            // Step 1: Generate a mnemonic phrase
            var mnemonicPhrase = _cryptoService.GenerateMnemonic();
            Assert.False(string.IsNullOrEmpty(mnemonicPhrase), "Mnemonic phrase should not be null or empty");

            // Step 2: Create a master key using the mnemonic phrase
            var (publicKey, privateKey, _) = _cryptoService.GenerateMasterKeyPair();
            Assert.NotNull(publicKey);
            Assert.NotNull(privateKey);

            // Step 3: Recover the master key using the same mnemonic phrase
            var (recoveredPublicKey, recoveredPrivateKey) = _cryptoService.GenerateMasterKeyPairFromMnemonic(mnemonicPhrase);
            Assert.NotNull(recoveredPublicKey);
            Assert.NotNull(recoveredPrivateKey);

            // Verify that the recovered keys match the original keys
            Assert.Equal(publicKey, recoveredPublicKey);
            Assert.Equal(privateKey, recoveredPrivateKey);
        }

        [Fact]
        public void RotateMasterKey_ShouldUpdateMasterKeyAndKeepTrackOfPreviousKeys()
        {
            // Arrange
            var mnemonicPhrase = _cryptoService.GenerateMnemonic();
            var (publicKey, privateKey, mnemonic) = _cryptoService.GenerateMasterKeyPairFromMnemonic(mnemonicPhrase);
            var masterKey = new MasterKey(publicKey, privateKey, mnemonic, null);

            // Act
            var rotatedMasterKey = _cryptoService.RotateMasterKey(masterKey);

            // Assert
            Assert.NotNull(rotatedMasterKey.ForwardMnemonic);
            Assert.NotNull(rotatedMasterKey.PublicKey);
            Assert.NotNull(rotatedMasterKey.PrivateKey);
            Assert.Contains(publicKey, rotatedMasterKey.PreviousPublicKeys); // Ensure the original public key is in the list of previous public keys
            Assert.NotEqual(publicKey, rotatedMasterKey.PublicKey); // Ensure the new public key is different from the original
        }
    }
}

```