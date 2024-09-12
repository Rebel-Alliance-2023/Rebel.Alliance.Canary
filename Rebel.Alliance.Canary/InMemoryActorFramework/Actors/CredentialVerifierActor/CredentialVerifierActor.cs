using System;
using System.Threading.Tasks;
using Rebel.Alliance.Canary.Abstractions;
using Rebel.Alliance.Canary.Abstractions.Actors;
using Rebel.Alliance.Canary.Models;
using Rebel.Alliance.Canary.Services;

namespace Rebel.Alliance.Canary.InMemoryActorFramework.Actors.CredentialVerifierActor
{

    public class CredentialVerifierActor : ActorBase, ICredentialVerifierActor
    {
        private readonly ICryptoService _cryptoService;
        private readonly IRevocationManagerActor _revocationManagerActor;

        public CredentialVerifierActor(string id, ICryptoService cryptoService, IRevocationManagerActor revocationManagerActor) : base(id)
        {
            _cryptoService = cryptoService ?? throw new ArgumentNullException(nameof(cryptoService));
            _revocationManagerActor = revocationManagerActor ?? throw new ArgumentNullException(nameof(revocationManagerActor));
        }

        public async Task<bool> VerifyCredentialAsync(VerifiableCredential credential)
        {
            if (credential == null || !credential.IsValid())
            {
                return false;
            }

            if (credential.IsExpired)
            {
                return false;
            }

            var isSignatureValid = await CheckSignatureAsync(credential);
            if (!isSignatureValid)
            {
                return false;
            }

            var isRevoked = await _revocationManagerActor.ValidateRevocationAsync(credential.Id);
            return !isRevoked;
        }

        private async Task<bool> CheckSignatureAsync(VerifiableCredential credential)
        {
            var credentialData = $"{credential.Issuer}|{credential.IssuanceDate}|{string.Join(",", credential.Claims)}";
            var publicKeyBytes = Convert.FromBase64String(credential.Proof.VerificationMethod);
            var signatureBytes = Convert.FromBase64String(credential.Proof.Jws);

            return await _cryptoService.VerifyDataAsync(publicKeyBytes, credentialData, signatureBytes);
        }
    }
}