using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rebel.Alliance.Canary.Verfiable.Credentials;

namespace Rebel.Alliance.Canary.Verfiable.Credentials
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class VerifiableCredential
    {
        // Standard JWT properties
        [JsonPropertyName("iss")]
        public string Issuer { get; set; }

        [JsonPropertyName("sub")]
        public string Subject { get; set; }

        [JsonPropertyName("aud")]
        public string Audience { get; set; }

        [JsonPropertyName("exp")]
        public DateTime ExpirationDate { get; set; }

        [JsonPropertyName("nbf")]
        public DateTime NotBefore { get; set; }

        [JsonPropertyName("iat")]
        public DateTime IssuanceDate { get; set; }

        [JsonPropertyName("jti")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        // Custom properties for VCs
        public Dictionary<string, string> Claims { get; set; }

        public Proof Proof { get; set; }

        [JsonPropertyName("vc_parent")]
        public string? ParentCredentialId { get; set; }

        // OIDC-specific properties (if needed)
        [JsonIgnore]
        public string Authority { get; set; }

        [JsonIgnore]
        public string ClientId { get; set; }

        [JsonIgnore]
        public string ClientSecret { get; set; }

        public VerifiableCredential(IHdCryptoService cryptoService)
        {
            Claims = new Dictionary<string, string>();
            Proof = new Proof(cryptoService);
            IssuanceDate = DateTime.UtcNow;
        }

        [JsonIgnore]
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
