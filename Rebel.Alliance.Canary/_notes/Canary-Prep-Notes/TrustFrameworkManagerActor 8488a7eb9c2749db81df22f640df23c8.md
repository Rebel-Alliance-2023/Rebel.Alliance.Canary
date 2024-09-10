# TrustFrameworkManagerActor

```csharp
using Dapr.Actors;
using Dapr.Actors.Runtime;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface ITrustFrameworkManagerActor : IActor
{
    Task<bool> RegisterIssuerAsync(string issuerDid, string publicKey);
    Task<bool> CertifyIssuerAsync(string issuerDid);
    Task<bool> RevokeIssuerAsync(string issuerDid);
    Task<bool> IsTrustedIssuerAsync(string issuerDid);
}

public class TrustFrameworkManagerActor : Actor, ITrustFrameworkManagerActor
{
    private readonly Dictionary<string, string> _issuers = new Dictionary<string, string>();
    private readonly Dictionary<string, bool> _certifiedIssuers = new Dictionary<string, bool>();

    public TrustFrameworkManagerActor(ActorHost host)
        : base(host)
    {
    }

    public async Task<bool> RegisterIssuerAsync(string issuerDid, string publicKey)
    {
        if (_issuers.ContainsKey(issuerDid))
        {
            this.Logger.LogWarning("Issuer already registered.");
            return false;
        }

        _issuers[issuerDid] = publicKey;
        this.Logger.LogInformation("Issuer registered successfully.");
        return true;
    }

    public async Task<bool> CertifyIssuerAsync(string issuerDid)
    {
        if (!_issuers.ContainsKey(issuerDid))
        {
            this.Logger.LogWarning("Issuer not found.");
            return false;
        }

        _certifiedIssuers[issuerDid] = true;
        this.Logger.LogInformation("Issuer certified successfully.");
        return true;
    }

    public async Task<bool> RevokeIssuerAsync(string issuerDid)
    {
        if (!_issuers.ContainsKey(issuerDid))
        {
            this.Logger.LogWarning("Issuer not found.");
            return false;
        }

        _certifiedIssuers[issuerDid] = false;
        this.Logger.LogInformation("Issuer revoked successfully.");
        return true;
    }

    public async Task<bool> IsTrustedIssuerAsync(string issuerDid)
    {
        return _certifiedIssuers.ContainsKey(issuerDid) && _certifiedIssuers[issuerDid];
    }
}

```