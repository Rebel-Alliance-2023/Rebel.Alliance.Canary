using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Rebel.Alliance.Canary.Abstractions;
using Rebel.Alliance.Canary.Abstractions.Actors;
using Rebel.Alliance.Canary.Models;
using Rebel.Alliance.Canary.Models.Rebel.Alliance.Canary.Models;
using Rebel.Alliance.Canary.Services;

namespace Rebel.Alliance.Canary.InMemoryActorFramework.Actors.VerifiableCredentialAsRootOfTrustActor
{

    public class VerifiableCredentialAsRootOfTrustActor : ActorBase, IVerifiableCredentialAsRootOfTrustActor
    {
        private readonly ICryptoService _cryptoService;
        private readonly IKeyManagementService _keyManagementService;

        public VerifiableCredentialAsRootOfTrustActor(
            string id,
            ICryptoService cryptoService,
            IKeyManagementService keyManagementService) : base(id)
        {
            _cryptoService = cryptoService ?? throw new ArgumentNullException(nameof(cryptoService));
            _keyManagementService = keyManagementService ?? throw new ArgumentNullException(nameof(keyManagementService));
        }

        public async Task<VerifiableCredential> CreateRootCredentialAsync(string issuerId, Dictionary<string, string> claims, string masterKeyId)
        {
            var masterKey = await _keyManagementService.GetMasterKeyAsync(masterKeyId);
            if (masterKey == null)
            {
                throw new InvalidOperationException("Master key not found");
            }

            var credential = new VerifiableCredential
            {
                Issuer = issuerId,
                IssuanceDate = DateTime.UtcNow,
                ExpirationDate = DateTime.UtcNow.AddYears(1),
                Claims = claims
            };

            var credentialData = $"{credential.Issuer}|{credential.IssuanceDate}|{string.Join(",", credential.Claims)}";
            var (signature, publicKey) = await _cryptoService.SignDataUsingIdentifierAsync(masterKeyId, credentialData);

            credential.Proof = new Proof
            {
                Type = "Ed25519Signature2018",
                Created = DateTime.UtcNow,
                VerificationMethod = Convert.ToBase64String(publicKey),
                ProofPurpose = "assertionMethod",
                Jws = Convert.ToBase64String(signature)
            };

            await StateManager.SetStateAsync("RootCredential", credential);

            return credential;
        }

        public async Task<VerifiableCredential> IssueSubordinateCredentialAsync(string issuerId, VerifiableCredential rootCredential, Dictionary<string, string> claims, string derivedKeyId)
        {
            if (!await VerifyCredentialChainAsync(rootCredential, rootCredential))
            {
                throw new InvalidOperationException("Root credential is not valid");
            }

            var derivedKey = await _keyManagementService.GetDerivedKeyAsync(derivedKeyId);
            if (derivedKey == null)
            {
                throw new InvalidOperationException("Derived key not found");
            }

            claims["ParentCredentialId"] = rootCredential.Id;
            var credential = new VerifiableCredential
            {
                Issuer = issuerId,
                IssuanceDate = DateTime.UtcNow,
                ExpirationDate = DateTime.UtcNow.AddYears(1),
                Claims = claims,
                ParentCredentialId = rootCredential.Id
            };

            var credentialData = $"{credential.Issuer}|{credential.IssuanceDate}|{string.Join(",", credential.Claims)}";
            var (signature, publicKey) = await _cryptoService.SignDataUsingIdentifierAsync(derivedKeyId, credentialData);

            credential.Proof = new Proof
            {
                Type = "Ed25519Signature2018",
                Created = DateTime.UtcNow,
                VerificationMethod = Convert.ToBase64String(publicKey),
                ProofPurpose = "assertionMethod",
                Jws = Convert.ToBase64String(signature)
            };

            return credential;
        }

        public async Task<bool> VerifyCredentialChainAsync(VerifiableCredential credential, VerifiableCredential rootCredential)
        {
            if (!await VerifyCredentialAsync(rootCredential))
            {
                return false;
            }

            var currentCredential = credential;
            while (!string.IsNullOrEmpty(currentCredential.ParentCredentialId))
            {
                if (!await VerifyCredentialAsync(currentCredential))
                {
                    return false;
                }

                currentCredential = await StateManager.GetStateAsync<VerifiableCredential>(currentCredential.ParentCredentialId);
            }

            return currentCredential.Id == rootCredential.Id;
        }

        private async Task<bool> VerifyCredentialAsync(VerifiableCredential credential)
        {
            var keyId = credential.Proof.VerificationMethod;
            byte[] publicKey = null;

            // Try to get the master key
            var masterKey = await _keyManagementService.GetMasterKeyAsync(keyId);
            if (masterKey != null)
            {
                publicKey = masterKey.PublicKey;
            }
            else
            {
                // If master key is not found, try to get the derived key
                var derivedKey = await _keyManagementService.GetDerivedKeyAsync(keyId);
                if (derivedKey != null)
                {
                    publicKey = derivedKey.PublicKey;
                }
            }

            if (publicKey == null)
            {
                return false;
            }

            var credentialData = $"{credential.Issuer}|{credential.IssuanceDate}|{string.Join(",", credential.Claims)}";
            var signature = Convert.FromBase64String(credential.Proof.Jws);

            return await _cryptoService.VerifyDataAsync(publicKey, credentialData, signature);
        }
    }
}