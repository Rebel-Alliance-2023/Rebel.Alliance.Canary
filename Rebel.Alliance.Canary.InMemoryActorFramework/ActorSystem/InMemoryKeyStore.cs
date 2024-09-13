using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Rebel.Alliance.Canary.Security;

namespace Rebel.Alliance.Canary.InMemoryActorFramework.ActorSystem
{
    public class InMemoryKeyStore : IKeyStore
    {
        private readonly ConcurrentDictionary<string, (byte[] PrivateKey, byte[] PublicKey)> _keys = new();

        public Task<string> StoreKeyAsync(string identifier, byte[] privateKey)
        {
            using var rsa = RSA.Create();
            rsa.ImportRSAPrivateKey(privateKey, out _);
            var publicKey = rsa.ExportRSAPublicKey();
            _keys[identifier] = (privateKey, publicKey);
            return Task.FromResult(identifier);
        }

        public Task<byte[]> RetrievePrivateKeyAsync(string identifier)
        {
            if (_keys.TryGetValue(identifier, out var keys))
            {
                return Task.FromResult(keys.PrivateKey);
            }

            throw new InvalidOperationException("Private key not found.");
        }

        public Task<byte[]> RetrievePublicKeyAsync(string identifier)
        {
            if (_keys.TryGetValue(identifier, out var keys))
            {
                return Task.FromResult(keys.PublicKey);
            }

            throw new InvalidOperationException("Public key not found.");
        }

        public Task<byte[]> SignDataAsync(string identifier, string data)
        {
            if (_keys.TryGetValue(identifier, out var keys))
            {
                using var rsa = RSA.Create();
                rsa.ImportRSAPrivateKey(keys.PrivateKey, out _);
                var dataBytes = Encoding.UTF8.GetBytes(data);
                var signature = rsa.SignData(dataBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                return Task.FromResult(signature);
            }

            throw new InvalidOperationException("Private key not found.");
        }
    }
}
