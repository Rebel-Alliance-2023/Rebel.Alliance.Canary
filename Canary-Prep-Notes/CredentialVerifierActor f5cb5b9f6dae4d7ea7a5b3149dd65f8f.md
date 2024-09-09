# CredentialVerifierActor

```csharp
using Dapr.Actors;
using Dapr.Actors.Runtime;
using SecureMessagingApp.Models;
using System;
using System.Text.Json;
using System.Threading.Tasks;

public interface ICredentialVerifierActor : IActor
{
    Task<bool> VerifyCredentialAsync(VerifiableCredential credential);
}

public class CredentialVerifierActor : Actor, ICredentialVerifierActor
{
    private readonly ICryptoService _cryptoService;
    private readonly IRevocationService _revocationService;

    public CredentialVerifierActor(ActorHost host, ICryptoService cryptoService, IRevocationService revocationService)
        : base(host)
    {
        _cryptoService = cryptoService;
        _revocationService = revocationService;
    }

    public async Task<bool> VerifyCredentialAsync(VerifiableCredential credential)
    {
        var credentialJson = SerializeCredential(credential, includeProof: false);
        var issuerPublicKey = Convert.FromBase64String(credential.Issuer);
        var signature = Convert.FromBase64String(credential.Proof.Jws);
        var isVerified = _cryptoService.VerifySignatureWithMasterOrDerivedKey(issuerPublicKey, credentialJson, signature);

        if (isVerified)
        {
            isVerified = !await _revocationService.IsCredentialRevokedAsync(credential.Id);
        }

        this.Logger.LogInformation("Credential verification result: {IsVerified}", isVerified);
        return isVerified;
    }

    private string SerializeCredential(VerifiableCredential credential, bool includeProof = true)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        if (!includeProof)
        {
            credential.Proof = null;
        }

        return JsonSerializer.Serialize(credential, options);
    }
}

```