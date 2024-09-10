# VerifiableCredentialAsRootOfTrustActor

## Interface

```csharp
using Dapr.Actors;
using System.Collections.Generic;
using System.Threading.Tasks;
using SecureMessagingApp.Models;

public interface IVerifiableCredentialAsRootOfTrustActor : IActor
{
    Task<VerifiableCredential> CreateRootCredentialAsync(string issuerId, Dictionary<string, string> claims, string masterKeyId);
    Task<VerifiableCredential> IssueSubordinateCredentialAsync(string issuerId, Dictionary<string, string> claims, string derivedKeyId);
    Task<bool> VerifyCredentialChainAsync(VerifiableCredential credential);
}

```

## Actor

```csharp
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Dapr.Actors;
using Dapr.Actors.Client;
using Dapr.Actors.Runtime;
using SecureMessagingApp.Models;
using SecureMessagingApp.Services;

public class VerifiableCredentialAsRootOfTrustActor : Actor, IVerifiableCredentialAsRootOfTrustActor
{
    private readonly ICryptoService _cryptoService;
    private readonly IKeyManagementService _keyManagementService;

    public VerifiableCredentialAsRootOfTrustActor(ActorHost host, ICryptoService cryptoService, IKeyManagementService keyManagementService)
        : base(host)
    {
        _cryptoService = cryptoService;
        _keyManagementService = keyManagementService;
    }

    private ICredentialIssuerActor GetCredentialIssuerActor(string issuerId)
    {
        var actorId = new ActorId(issuerId);
        return ActorProxy.Create<ICredentialIssuerActor>(actorId, nameof(CredentialIssuerActor));
    }

    private ICredentialVerifierActor GetCredentialVerifierActor(string verifierId)
    {
        var actorId = new ActorId(verifierId);
        return ActorProxy.Create<ICredentialVerifierActor>(actorId, nameof(CredentialVerifierActor));
    }

    protected override async Task OnActivateAsync()
    {
        Console.WriteLine($"Activating actor id: {this.Id}");

        // Optionally initialize state if it doesn't already exist
        var exists = await StateManager.ContainsStateAsync("RootCredential");
        if (!exists)
        {
            await StateManager.SetStateAsync("RootCredential", new VerifiableCredential());
        }
    }

    public async Task<VerifiableCredential> CreateRootCredentialAsync(string issuerId, Dictionary<string, string> claims, string masterKeyId)
    {
        var masterKey = await _keyManagementService.GetMasterKeyAsync(masterKeyId);
        if (masterKey == null)
        {
            throw new InvalidOperationException("Master key not found");
        }

        var credentialIssuer = GetCredentialIssuerActor(issuerId);
        var credential = await credentialIssuer.IssueCredentialAsync(issuerId, claims);
        credential.SigningKeyId = masterKeyId;

        // Sign the credential with the master key
        var credentialJson = JsonSerializer.Serialize(credential);
        var signature = _cryptoService.SignData(masterKey.PrivateKey, credentialJson);
        credential.Proof = new Proof
        {
            Created = DateTime.UtcNow,
            VerificationMethod = Convert.ToBase64String(masterKey.PublicKey),
            Jws = Convert.ToBase64String(signature)
        };

        await StateManager.SetStateAsync("RootCredential", credential);
        return credential;
    }

    public async Task<VerifiableCredential> IssueSubordinateCredentialAsync(string issuerId, Dictionary<string, string> claims, string derivedKeyId)
    {
        var rootCredential = await StateManager.GetStateAsync<VerifiableCredential>("RootCredential");
        if (!await VerifyCredentialAsync(rootCredential))
        {
            throw new InvalidOperationException("Root credential is not valid");
        }

        var derivedKey = await _keyManagementService.GetDerivedKeyAsync(derivedKeyId);
        if (derivedKey == null)
        {
            throw new InvalidOperationException("Derived key not found");
        }

        // Add the ParentCredentialId to the claims
        claims["ParentCredentialId"] = rootCredential.Id;

        var credentialIssuer = GetCredentialIssuerActor(issuerId);
        var credential = await credentialIssuer.IssueCredentialAsync(issuerId, claims);
        credential.SigningKeyId = derivedKeyId;
        credential.ParentCredentialId = rootCredential.Id;

        // Sign the credential with the derived key
        var credentialJson = JsonSerializer.Serialize(credential);
        var signature = _cryptoService.SignData(derivedKey.PrivateKey, credentialJson);
        credential.Proof = new Proof
        {
            Created = DateTime.UtcNow,
            VerificationMethod = Convert.ToBase64String(derivedKey.PublicKey),
            Jws = Convert.ToBase64String(signature)
        };

        return credential;
    }

    public async Task<bool> VerifyCredentialChainAsync(VerifiableCredential credential)
    {
        var rootCredential = await StateManager.GetStateAsync<VerifiableCredential>("RootCredential");
        if (!await VerifyCredentialAsync(rootCredential))
        {
            return false;
        }

        // Verify the chain up to the root credential
        var currentCredential = credential;
        while (!string.IsNullOrEmpty(currentCredential.ParentCredentialId))
        {
            if (!await VerifyCredentialAsync(currentCredential))
            {
                return false;
            }

            // Fetch the parent credential (simplified here; you would need to implement the actual retrieval)
            currentCredential = await GetCredentialByIdAsync(currentCredential.ParentCredentialId);
        }

        // Ensure the chain ends with the root credential
        return currentCredential.Id == rootCredential.Id;
    }

    private async Task<bool> VerifyCredentialAsync(VerifiableCredential credential)
    {
        var keyId = credential.SigningKeyId;
        var key = await _keyManagementService.GetMasterKeyAsync(keyId) as dynamic ?? await _keyManagementService.GetDerivedKeyAsync(keyId) as dynamic;

        if (key == null)
        {
            return false;
        }

        var credentialJson = JsonSerializer.Serialize(credential, new JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull });
        var signature = Convert.FromBase64String(credential.Proof.Jws);

        return _cryptoService.VerifyData(key.PublicKey, credentialJson, signature);
    }

    private async Task<VerifiableCredential> GetCredentialByIdAsync(string credentialId)
    {
        // Implement the logic to retrieve the credential by its ID from a database or another storage mechanism
        throw new NotImplementedException();
    }
}

```