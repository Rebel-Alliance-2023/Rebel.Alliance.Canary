using Rebel.Alliance.Canary.Models.Rebel.Alliance.Canary.Models;
using System;
using System.Collections.Generic;

namespace Rebel.Alliance.Canary.Models
{
    public class VerifiableCredential
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Issuer { get; set; }
        public string Subject { get; set; }
        public DateTime IssuanceDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public Dictionary<string, string> Claims { get; set; }
        public Proof Proof { get; set; }
        public string? ParentCredentialId { get; set; }

        public VerifiableCredential()
        {
            Claims = new Dictionary<string, string>();
            Proof = new Proof();
            IssuanceDate = DateTime.UtcNow;
        }

        public bool IsExpired => DateTime.UtcNow > ExpirationDate;

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(Issuer) &&
                   !string.IsNullOrEmpty(Subject) &&
                   IssuanceDate != default &&
                   ExpirationDate > IssuanceDate &&
                   Proof != null &&
                   Proof.IsValid();
        }
    }
}