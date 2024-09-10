# CredentialIssuerActor

```csharp
using Dapr.Actors;
using Dapr.Actors.Runtime;
using SecureMessagingApp.Models;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

public interface ICredentialIssuerActor : IActor
{
    Task<VerifiableCredential> IssueCredentialAsync(string subjectDid, Dictionary<string, string> claims);
}

public class CredentialIssuerActor : Actor, ICredentialIssuerActor
{
    private readonly ICryptoService _cryptoService;
    private readonly ITrustFrameworkService _trustFrameworkService;

    public CredentialIssuerActor(ActorHost host, ICryptoService cryptoService, ITrustFrameworkService trustFrameworkService)
        : base(host)
    {
        _cryptoService = cryptoService;
        _trustFrameworkService = trustFrameworkService;
    }

public async Task<VerifiableCredential> IssueCredentialAsync(string subjectDid, Dictionary<string, string> claims)
{
    var issuerDid = this.Id.GetId();
    if (!_trustFrameworkService.IsTrustedIssuer(issuerDid))
    {
        throw new InvalidOperationException("Issuer is not trusted");
    }

    var issuerKey = await _cryptoService.GetIssuerKeyAsync(issuerDid);

    var credential = new VerifiableCredential
    {
        Id = $"urn:uuid:{Guid.NewGuid()}",
        Type = new List<string> { "VerifiableCredential", "CustomCredential" },
        Issuer = issuerDid,
        IssuanceDate = DateTime.UtcNow,
        CredentialSubject = new CredentialSubject
        {
            Id = subjectDid,
            Name = claims["name"],
            Email = claims["email"],
            Birthdate = claims["birthdate"],
            Address = new Address
            {
                Street = claims["street"],
                City = claims["city"],
                State = claims["state"],
                PostalCode = claims["postalCode"],
                Country = claims["country"]
            },
            Membership = new Membership
            {
                Organization = claims["organization"],
                Role = claims["role"],
                MemberSince = claims["memberSince"]
            }
        }
    };

    var credentialJson = JsonSerializer.Serialize(credential);
    var signature = _cryptoService.SignDataWithMasterKey(issuerKey.PrivateKey, credentialJson);
    var jws = Convert.ToBase64String(signature);

    credential.Proof = new Proof
    {
        Type = "Ed25519Signature2018",
        Created = DateTime.UtcNow,
        ProofPurpose = "assertionMethod",
        VerificationMethod = Convert.ToBase64String(issuerKey.PublicKey),
        Jws = jws
    };

    this.Logger.LogInformation("Credential issued successfully.");
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