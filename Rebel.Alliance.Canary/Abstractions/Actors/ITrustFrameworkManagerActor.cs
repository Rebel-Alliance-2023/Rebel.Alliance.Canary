namespace Rebel.Alliance.Canary.Abstractions.Actors
{
    public interface ITrustFrameworkManagerActor : IActor
    {
        Task<bool> RegisterIssuerAsync(string issuerDid, string publicKey);
        Task<bool> CertifyIssuerAsync(string issuerDid);
        Task<bool> RevokeIssuerAsync(string issuerDid);
        Task<bool> IsTrustedIssuerAsync(string issuerDid);
    }
}
