# Key Management Service

Derive key pairs from a master key pair, where these derived key pairs share a trait that identifies them as being derived from the master key. This concept is often implemented using Hierarchical Deterministic (HD) keys, which are commonly used in cryptocurrency wallets.

HD keys are derived from a single master key (also known as the root key) in a tree structure. Each derived key can be traced back to the master key, and they share a common trait or identifier.

## Interface

```csharp
using System.Threading.Tasks;

public interface IKeyManagementService
{
    Task<MasterKey> GenerateMasterKeyAsync();
    Task<DerivedKey> GenerateDerivedKeyAsync(string masterKeyId);
    Task<MasterKey> GetMasterKeyAsync(string keyId);
    Task<DerivedKey> GetDerivedKeyAsync(string keyId);
}

```

## Service

```csharp
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

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
        var (publicKey, privateKey) = _cryptoService.GenerateKeyPair();
        var masterKey = new MasterKey
        {
            PublicKey = publicKey,
            PrivateKey = privateKey,
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

        var (publicKey, privateKey) = _cryptoService.GenerateKeyPair();
        var derivedKey = new DerivedKey
        {
            PublicKey = publicKey,
            PrivateKey = privateKey,
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

```