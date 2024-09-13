using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Rebel.Alliance.Canary.Abstractions;
using Rebel.Alliance.Canary.Abstractions.Actors;
using Rebel.Alliance.Canary.Models.Rebel.Alliance.Canary.Models;
using Rebel.Alliance.Canary.Security;
using Rebel.Alliance.Canary.VerifiableCredentials;

namespace Rebel.Alliance.Canary.InMemoryActorFramework.Actors.VerifiableCredentialActor
{

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
                    Type = "Ed25519Signature2018",
                    Created = DateTime.UtcNow,
                    VerificationMethod = Convert.ToBase64String(publicKey),
                    ProofPurpose = "assertionMethod",
                    Jws = Convert.ToBase64String(signature)
                };

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error signing credential: {ex.Message}");
                return false;
            }
        }

        public async Task<VerifiableCredential> IssueCredentialAsync(string issuerId, string subjectId, DateTime expirationDate, Dictionary<string, string> claims)
        {
            var credential = new VerifiableCredential
            {
                Issuer = issuerId,
                Subject = subjectId,
                IssuanceDate = DateTime.UtcNow,
                ExpirationDate = expirationDate,
                Claims = claims
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
}