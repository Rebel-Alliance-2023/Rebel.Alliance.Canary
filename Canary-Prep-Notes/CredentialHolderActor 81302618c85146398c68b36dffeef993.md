# CredentialHolderActor

```csharp
using Dapr.Actors;
using Dapr.Actors.Runtime;
using SecureMessagingApp.Models;
using System.Threading.Tasks;

public interface ICredentialHolderActor : IActor
{
    Task StoreCredentialAsync(VerifiableCredential credential);
    Task<VerifiableCredential> PresentCredentialAsync(string credentialId);
    Task<VerifiableCredential> RenewCredentialAsync(string credentialId);
}

public class CredentialHolderActor : Actor, ICredentialHolderActor
{
    private readonly ICryptoService _cryptoService;

    public CredentialHolderActor(ActorHost host, ICryptoService cryptoService)
        : base(host)
    {
        _cryptoService = cryptoService;
    }

    public async Task StoreCredentialAsync(VerifiableCredential credential)
    {
        await this.StateManager.SetStateAsync(credential.Id, credential);
        this.Logger.LogInformation("Credential stored successfully.");
    }

    public async Task<VerifiableCredential> PresentCredentialAsync(string credentialId)
    {
        var credential = await this.StateManager.GetStateAsync<VerifiableCredential>(credentialId);
        return credential;
    }

    public async Task<VerifiableCredential> RenewCredentialAsync(string credentialId)
    {
        var credential = await this.StateManager.GetStateAsync<VerifiableCredential>(credentialId);
        var issuerDid = credential.Issuer;

        var issuerKey = await _cryptoService.GetIssuerKeyAsync(issuerDid);

        // Re-issue the credential
        credential.IssuanceDate = DateTime.UtcNow;
        credential.ExpiresAt = DateTime.UtcNow.AddYears(1);

        var credentialJson = SerializeCredential(credential, includeProof: false);
        var signature = _cryptoService.SignDataWithMasterKey(issuerKey.PrivateKey, credentialJson);
        var jws = Convert.ToBase64String(signature);

        credential.Proof = new Proof
        {
            Created = DateTime.UtcNow,
            VerificationMethod = Convert.ToBase64String(issuerKey.PublicKey),
            Jws = jws
        };

        await this.StateManager.SetStateAsync(credential.Id, credential);
        this.Logger.LogInformation("Credential renewed successfully.");
        return credential;
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