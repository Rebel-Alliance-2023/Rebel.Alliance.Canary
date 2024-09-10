using Rebel.Alliance.Canary.Abstractions;
using Rebel.Alliance.Canary.Models;
using Rebel.Alliance.Canary.Services;

public interface IVerifiableCredentialAsRootOfTrustActor : IActor
{
    Task<VerifiableCredential> CreateRootCredentialAsync(string issuerId, Dictionary<string, string> claims, string masterKeyId);
    Task<VerifiableCredential> IssueSubordinateCredentialAsync(string issuerId, VerifiableCredential rootCredential, Dictionary<string, string> claims, string derivedKeyId);
    Task<bool> VerifyCredentialChainAsync(VerifiableCredential credential, VerifiableCredential rootCredential);
}

public class VerifiableCredentialAsRootOfTrustActor : ActorBase, IVerifiableCredentialAsRootOfTrustActor
{
    private readonly ICryptoService _cryptoService;
    private readonly IKeyManagementService _keyManagementService;

    public VerifiableCredentialAsRootOfTrustActor(
        string id,
        ICryptoService cryptoService,
        IKeyManagementService keyManagementService) : base(id)
    {
        _cryptoService = cryptoService;
        _keyManagementService = keyManagementService;
    }

    public async Task<VerifiableCredential> CreateRootCredentialAsync(string issuerId, Dictionary<string, string> claims, string masterKeyId)
    {
        var masterKey = await _keyManagementService.GetMasterKeyAsync(masterKeyId);
        if (masterKey == null)
        {
            throw new InvalidOperationException("Master key not found");
        }

        var credential = new VerifiableCredential
        {
            Id = Guid.NewGuid().ToString(),
            Issuer = issuerId,
            IssuanceDate = DateTime.UtcNow,
            Claims = claims
        };

        var credentialData = $"{credential.Issuer}|{credential.IssuanceDate}|{string.Join(",", credential.Claims)}";
        var (signature, publicKey) = await _cryptoService.SignDataUsingIdentifierAsync(masterKeyId, credentialData);

        credential.Proof = new Proof
        {
            Created = DateTime.UtcNow,
            VerificationMethod = Convert.ToBase64String(publicKey),
            Jws = Convert.ToBase64String(signature)
        };

        await StateManager.SetStateAsync("RootCredential", credential);

        return credential;
    }

    public async Task<VerifiableCredential> IssueSubordinateCredentialAsync(string issuerId, VerifiableCredential rootCredential, Dictionary<string, string> claims, string derivedKeyId)
    {
        if (!await VerifyCredentialChainAsync(rootCredential, rootCredential))
        {
            throw new InvalidOperationException("Root credential is not valid");
        }

        var derivedKey = await _keyManagementService.GetDerivedKeyAsync(derivedKeyId);
        if (derivedKey == null)
        {
            throw new InvalidOperationException("Derived key not found");
        }

        claims["ParentCredentialId"] = rootCredential.Id;
        var credential = new VerifiableCredential
        {
            Id = Guid.NewGuid().ToString(),
            Issuer = issuerId,
            IssuanceDate = DateTime.UtcNow,
            Claims = claims,
            ParentCredentialId = rootCredential.Id,
            Proof = new Proof
            {
                VerificationMethod = Convert.ToBase64String(derivedKey.PublicKey)
            }
        };

        var credentialData = $"{credential.Issuer}|{credential.IssuanceDate}|{string.Join(",", credential.Claims)}";
        var (signature, publicKey) = await _cryptoService.SignDataUsingIdentifierAsync(derivedKeyId, credentialData);

        credential.Proof.Jws = Convert.ToBase64String(signature);
        credential.Proof.Created = DateTime.UtcNow;

        return credential;
    }

    public async Task<bool> VerifyCredentialChainAsync(VerifiableCredential credential, VerifiableCredential rootCredential)
    {
        if (!await VerifyCredentialAsync(rootCredential))
        {
            return false;
        }

        var currentCredential = credential;
        while (!string.IsNullOrEmpty(currentCredential.ParentCredentialId))
        {
            if (!await VerifyCredentialAsync(currentCredential))
            {
                return false;
            }

            currentCredential = await StateManager.GetStateAsync<VerifiableCredential>(currentCredential.ParentCredentialId);
        }

        return currentCredential.Id == rootCredential.Id;
    }

    private async Task<bool> VerifyCredentialAsync(VerifiableCredential credential)
    {
        var keyId = credential.Proof.VerificationMethod;
        var key = await _keyManagementService.GetMasterKeyAsync(keyId);
        if (key == null) {
            await _keyManagementService.GetDerivedKeyAsync(keyId);
        }

        if (key == null)
        {
            return false;
        }

        var credentialData = $"{credential.Issuer}|{credential.IssuanceDate}|{string.Join(",", credential.Claims)}";
        var signature = Convert.FromBase64String(credential.Proof.Jws);

        return await _cryptoService.VerifyDataAsync(key.PublicKey, credentialData, signature);
    }
}
