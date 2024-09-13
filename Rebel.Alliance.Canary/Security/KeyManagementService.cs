using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Rebel.Alliance.Canary.Security
{
    public interface IKeyManagementService
    {
        Task<MasterKey> GenerateMasterKeyAsync();
        Task<DerivedKey> GenerateDerivedKeyAsync(string masterKeyId);
        Task<MasterKey> GetMasterKeyAsync(string keyId);
        Task<DerivedKey> GetDerivedKeyAsync(string keyId);
    }

    public class KeyManagementService : IKeyManagementService
    {
        private readonly ICryptoService _cryptoService;
        private readonly ConcurrentDictionary<string, MasterKey> _masterKeys = new();
        private readonly ConcurrentDictionary<string, DerivedKey> _derivedKeys = new();

        public KeyManagementService(ICryptoService cryptoService)
        {
            _cryptoService = cryptoService;
        }

        public async Task<MasterKey> GenerateMasterKeyAsync()
        {
            var keyPair = await _cryptoService.GenerateKeyPairAsync();
            var masterKey = new MasterKey
            {
                PublicKey = keyPair.PublicKey,
                PrivateKey = keyPair.PrivateKey,
                Id = Guid.NewGuid().ToString()
            };
            _masterKeys[masterKey.Id] = masterKey;
            return masterKey;
        }

        public async Task<DerivedKey> GenerateDerivedKeyAsync(string masterKeyId)
        {
            if (!_masterKeys.TryGetValue(masterKeyId, out var masterKey))
            {
                throw new InvalidOperationException("Master key not found");
            }

            var keyPair = await _cryptoService.GenerateKeyPairAsync();
            var derivedKey = new DerivedKey
            {
                PublicKey = keyPair.PublicKey,
                PrivateKey = keyPair.PrivateKey,
                Id = Guid.NewGuid().ToString(),
                MasterKeyId = masterKeyId
            };
            masterKey.DerivedKeys.Add(derivedKey);
            _derivedKeys[derivedKey.Id] = derivedKey;
            return derivedKey;
        }

        public async Task<MasterKey> GetMasterKeyAsync(string keyId)
        {
            _masterKeys.TryGetValue(keyId, out var masterKey);
            return masterKey;
        }

        public async Task<DerivedKey> GetDerivedKeyAsync(string keyId)
        {
            _derivedKeys.TryGetValue(keyId, out var derivedKey);
            return derivedKey;
        }
    }
}
