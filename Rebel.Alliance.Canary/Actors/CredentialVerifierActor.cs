using System;
using System.Threading.Tasks;
using Rebel.Alliance.Canary.Abstractions;
using Rebel.Alliance.Canary.Actors;
using Rebel.Alliance.Canary.Models;
using Rebel.Alliance.Canary.Services;

public interface ICredentialVerifierActor : IActor
{
    Task<bool> VerifyCredentialAsync(VerifiableCredential credential);
}

public class CredentialVerifierActor : ActorBase, ICredentialVerifierActor
{
    private readonly ICryptoService _cryptoService;
    private readonly IRevocationManagerActor _revocationManagerActor;

    public CredentialVerifierActor(string id, ICryptoService cryptoService, IRevocationManagerActor revocationManagerActor) : base(id)
    {
        _cryptoService = cryptoService;
        _revocationManagerActor = revocationManagerActor;
    }

    public async Task<bool> VerifyCredentialAsync(VerifiableCredential credential)
    {
        // Verify the signature of the credential asynchronously
        var isSignatureValid = await CheckSignatureAsync(credential);
        if (!isSignatureValid)
        {
            return false;
        }

        // Check if the credential has been revoked
        var isRevoked = await _revocationManagerActor.ValidateRevocationAsync(credential.Id);
        return !isRevoked;
    }

    private async Task<bool> CheckSignatureAsync(VerifiableCredential credential)
    {
        var credentialData = $"{credential.Issuer}|{credential.IssuanceDate}|{string.Join(",", credential.Claims)}";
        var publicKeyBytes = Convert.FromBase64String(credential.Proof.VerificationMethod);
        var signatureBytes = Convert.FromBase64String(credential.Proof.Jws);

        // Verify the signature asynchronously
        return await _cryptoService.VerifyDataAsync(publicKeyBytes, credentialData, signatureBytes);
    }
}
