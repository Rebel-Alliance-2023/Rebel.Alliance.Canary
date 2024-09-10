using System;
using System.Threading.Tasks;
using Rebel.Alliance.Canary.Abstractions;
using Rebel.Alliance.Canary.Models;
using Rebel.Alliance.Canary.Services;

public interface IVerifiableCredentialActor : IActor
{
    Task<bool> SignCredentialAsync(VerifiableCredential credential);
    Task<VerifiableCredential> IssueCredentialAsync(string issuerId, string subjectId, DateTime issuanceDate, DateTime expirationDate);
}

public class VerifiableCredentialActor : ActorBase, IVerifiableCredentialActor
{
    private readonly ICryptoService _cryptoService;

    public VerifiableCredentialActor(string id, ICryptoService cryptoService) : base(id)
    {
        _cryptoService = cryptoService;
    }

    public async Task<bool> SignCredentialAsync(VerifiableCredential credential)
    {
        var credentialData = $"{credential.Issuer}|{credential.IssuanceDate}|{string.Join(",", credential.Claims)}";

        // Call the method to get both the signature and public key
        var (signature, publicKey) = await _cryptoService.SignDataUsingIdentifierAsync(credential.Issuer, credentialData);

        credential.Proof = new Proof
        {
            VerificationMethod = Convert.ToBase64String(publicKey),
            Jws = Convert.ToBase64String(signature)
        };

        return true;
    }

    public async Task<VerifiableCredential> IssueCredentialAsync(string issuerId, string subjectId, DateTime issuanceDate, DateTime expirationDate)
    {
        var credential = new VerifiableCredential
        {
            Id = Guid.NewGuid().ToString(),
            Issuer = issuerId,
            Subject = subjectId,
            IssuanceDate = issuanceDate,
            ExpirationDate = expirationDate,
            Claims = new Dictionary<string, string>()
        };

        var signed = await SignCredentialAsync(credential);
        if (!signed)
        {
            throw new InvalidOperationException("Credential could not be signed.");
        }

        // Persist the credential in state management
        await StateManager.SetStateAsync("Credential", credential);

        return credential;
    }
}
