# RevocationManagerActor

```csharp
using Dapr.Actors;
using Dapr.Actors.Runtime;
using System.Threading.Tasks;

public interface IRevocationManagerActor : IActor
{
    Task RevokeCredentialAsync(string credentialId);
    Task<bool> IsCredentialRevokedAsync(string credentialId);
}

public class RevocationManagerActor : Actor, IRevocationManagerActor
{
    public RevocationManagerActor(ActorHost host)
        : base(host)
    {
    }

    public async Task RevokeCredentialAsync(string credentialId)
    {
        await this.StateManager.SetStateAsync(credentialId, true);
        this.Logger.LogInformation("Credential revoked successfully.");
    }

    public async Task<bool> IsCredentialRevokedAsync(string credentialId)
    {
        return await this.StateManager.GetStateAsync<bool>(credentialId);
    }
}

```