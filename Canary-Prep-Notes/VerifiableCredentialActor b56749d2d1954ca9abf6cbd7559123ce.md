# VerifiableCredentialActor

## Interface

```csharp
using Dapr.Actors;
using System.Collections.Generic;
using System.Threading.Tasks;
using SecureMessagingApp.Models;

public interface IVerifiableCredentialActor : IActor
{
    Task<VerifiableCredential> CreateCredentialAsync(MasterKey issuerKey, string subject, Dictionary<string, string> credentialData);
    Task<bool> VerifyCredentialAsync(VerifiableCredential credential);
}

```

## Actor

```csharp
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Dapr.Actors.Runtime;
using Microsoft.Extensions.Logging;
using SecureMessagingApp.Models;
using SecureMessagingApp.Services;

public class VerifiableCredentialActor : Actor, IVerifiableCredentialActor
{
    private readonly ICryptoService _cryptoService;
    private readonly ILogger<VerifiableCredentialActor> _logger;

    public VerifiableCredentialActor(ActorHost host, ICryptoService cryptoService, ILogger<VerifiableCredentialActor> logger)
        : base(host)
    {
        _cryptoService = cryptoService;
        _logger = logger;
    }

    public async Task<VerifiableCredential> CreateCredentialAsync(MasterKey issuerKey, string subject, Dictionary<string, string> credentialData)
    {
        try
        {
            var credential = new VerifiableCredential
            {
                Issuer = Convert.ToBase64String(issuerKey.PublicKey),
                Subject = subject,
                CredentialData = credentialData,
                IssuedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddYears(1) // Example: Credentials are valid for 1 year
            };

            // Serialize credential data to a string
            var credentialString = SerializeCredential(credential);

            // Sign the credential data
            var signature = _cryptoService.SignData(issuerKey.PrivateKey, credentialString);
            credential.Signature = signature;

            _logger.LogInformation("Credential created and signed by issuer.");
            return credential;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating credential.");
            throw;
        }
    }

    public async Task<bool> VerifyCredentialAsync(VerifiableCredential credential)
    {
        try
        {
            // Serialize credential data to a string (excluding the signature)
            var credentialString = SerializeCredential(credential, includeSignature: false);

            // Verify the credential signature
            var issuerPublicKey = Convert.FromBase64String(credential.Issuer);
            var isVerified = _cryptoService.VerifyData(issuerPublicKey, credentialString, credential.Signature);

            _logger.LogInformation("Credential verification result: {IsVerified}", isVerified);
            return isVerified;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying credential.");
            throw;
        }
    }

    private string SerializeCredential(VerifiableCredential credential, bool includeSignature = true)
    {
        var credentialString = $"Issuer:{credential.Issuer},Subject:{credential.Subject},IssuedAt:{credential.IssuedAt},ExpiresAt:{credential.ExpiresAt}";
        foreach (var item in credential.CredentialData)
        {
            credentialString += $",{item.Key}:{item.Value}";
        }

        if (includeSignature && credential.Signature != null)
        {
            credentialString += $",Signature:{Convert.ToBase64String(credential.Signature)}";
        }

        return credentialString;
    }
}

```