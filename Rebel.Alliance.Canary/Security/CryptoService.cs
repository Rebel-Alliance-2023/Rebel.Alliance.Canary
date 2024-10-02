using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Rebel.Alliance.Canary.Security
{
    public interface ICryptoService
    {
        Task<(byte[] PublicKey, byte[] PrivateKey)> GenerateKeyPairAsync();
        Task<byte[]> SignDataAsync(byte[] privateKey, string data);
        Task<bool> VerifyDataAsync(byte[] publicKey, string data, byte[] signature);
        Task<string> GenerateMnemonicAsync();
        Task<byte[]> EncryptDataAsync(byte[] publicKey, string data);
        Task<string> DecryptDataAsync(byte[] privateKey, byte[] data);
        Task<(byte[] Signature, byte[] PublicKey)> SignDataUsingIdentifierAsync(string identifier, string data);

    }

    public class CryptoService : ICryptoService
    {
        private readonly IKeyStore _keyStore;

        public CryptoService(IKeyStore keyStore)
        {
            _keyStore = keyStore;
        }

        public async Task<(byte[] PublicKey, byte[] PrivateKey)> GenerateKeyPairAsync()
        {
            return await Task.Run(() =>
            {
                using var rsa = RSA.Create();
                return (rsa.ExportRSAPublicKey(), rsa.ExportRSAPrivateKey());
            });
        }

        public async Task<byte[]> SignDataAsync(byte[] privateKey, string data)
        {
            return await Task.Run(() =>
            {
                using var rsa = RSA.Create();
                rsa.ImportRSAPrivateKey(privateKey, out _);
                var dataBytes = Encoding.UTF8.GetBytes(data);
                return rsa.SignData(dataBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            });
        }

        public async Task<bool> VerifyDataAsync(byte[] publicKey, string data, byte[] signature)
        {
            return await Task.Run(() =>
            {
                using var rsa = RSA.Create();
                rsa.ImportRSAPublicKey(publicKey, out _);
                var dataBytes = Encoding.UTF8.GetBytes(data);
                return rsa.VerifyData(dataBytes, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            });
        }

        public async Task<string> GenerateMnemonicAsync()
        {
            return await Task.FromResult("Example mnemonic phrase");
        }

        public async Task<byte[]> EncryptDataAsync(byte[] publicKey, string data)
        {
            return await Task.Run(() =>
            {
                using var rsa = RSA.Create();
                rsa.ImportRSAPublicKey(publicKey, out _);
                var dataBytes = Encoding.UTF8.GetBytes(data);
                return rsa.Encrypt(dataBytes, RSAEncryptionPadding.OaepSHA256);
            });
        }

        public async Task<string> DecryptDataAsync(byte[] privateKey, byte[] data)
        {
            return await Task.Run(() =>
            {
                using var rsa = RSA.Create();
                rsa.ImportRSAPrivateKey(privateKey, out _);
                var decryptedBytes = rsa.Decrypt(data, RSAEncryptionPadding.OaepSHA256);
                return Encoding.UTF8.GetString(decryptedBytes);
            });
        }

        public async Task<(byte[] Signature, byte[] PublicKey)> SignDataUsingIdentifierAsync(string identifier, string data)
        {
            // Retrieve the private key using the identifier from the key store
            var privateKey = await _keyStore.RetrievePrivateKeyAsync(identifier);

            // Sign the data
            var signature = await Task.Run(() => SignDataAsync(privateKey, data));

            // Retrieve the public key
            var publicKey = await _keyStore.RetrievePublicKeyAsync(identifier);

            return (signature, publicKey);
        }
    }
}
