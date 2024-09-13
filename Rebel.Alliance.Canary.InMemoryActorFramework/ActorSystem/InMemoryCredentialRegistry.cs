namespace Rebel.Alliance.Canary.InMemoryActorFramework.ActorSystem
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using Rebel.Alliance.Canary.VerifiableCredentials;

    public class InMemoryCredentialRegistry
    {
        private readonly ConcurrentDictionary<string, VerifiableCredential> _issuedCredentials = new();
        private readonly ConcurrentDictionary<string, VerifiableCredential> _revokedCredentials = new();
        private readonly ConcurrentDictionary<string, string> _trustedIssuers = new();

        public bool StoreCredential(VerifiableCredential credential)
        {
            if (credential == null || string.IsNullOrEmpty(credential.Id))
            {
                throw new ArgumentException("Credential must have a valid ID");
            }

            return _issuedCredentials.TryAdd(credential.Id, credential);
        }

        public bool RevokeCredential(string credentialId)
        {
            if (!_issuedCredentials.TryRemove(credentialId, out var credential))
            {
                return false;
            }

            return _revokedCredentials.TryAdd(credentialId, credential);
        }

        public bool IsCredentialRevoked(string credentialId)
        {
            return _revokedCredentials.ContainsKey(credentialId);
        }

        public bool RegisterTrustedIssuer(string issuerId, string publicKey)
        {
            if (string.IsNullOrEmpty(issuerId) || string.IsNullOrEmpty(publicKey))
            {
                throw new ArgumentException("Issuer ID and public key cannot be null or empty");
            }

            return _trustedIssuers.TryAdd(issuerId, publicKey);
        }

        public bool IsTrustedIssuer(string issuerId)
        {
            return _trustedIssuers.ContainsKey(issuerId);
        }

        public VerifiableCredential GetCredential(string credentialId)
        {
            _issuedCredentials.TryGetValue(credentialId, out var credential);
            return credential;
        }
    }
}
