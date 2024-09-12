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
        _cryptoService = cryptoService ?? throw new ArgumentNullException(nameof(cryptoService));
    }

    public async Task<bool> SignCredentialAsync(VerifiableCredential credential)
    {
        if (credential == null)
        {
            throw new ArgumentNullException(nameof(credential));
        }

        var credentialData = $"{credential.Issuer}|{credential.IssuanceDate}|{string.Join(",", credential.Claims)}";

        try
        {
            var (signature, publicKey) = await _cryptoService.SignDataUsingIdentifierAsync(credential.Issuer, credentialData);

            credential.Proof = new Proof
            {
                VerificationMethod = Convert.ToBase64String(publicKey),
                Jws = Convert.ToBase64String(signature),
                Created = DateTime.UtcNow
            };

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error signing credential: {ex.Message}");
            return false;
        }
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

        await StateManager.SetStateAsync($"Credential:{credential.Id}", credential);

        return credential;
    }
}
