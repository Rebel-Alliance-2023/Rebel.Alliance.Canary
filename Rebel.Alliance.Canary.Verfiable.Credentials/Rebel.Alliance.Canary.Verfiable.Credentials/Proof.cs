namespace Rebel.Alliance.Canary.Verfiable.Credentials
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;

        public class Proof
        {
            [Required]
            [JsonPropertyName("type")]
            public string Type { get; set; } = "Ed25519Signature2018";

            [Required]
            [JsonPropertyName("created")]
            public DateTime Created { get; set; }

            [Required]
            [JsonPropertyName("verificationMethod")]
            public string VerificationMethod { get; set; }

            [Required]
            [JsonPropertyName("proofPurpose")]
            public string ProofPurpose { get; set; } = "assertionMethod";

            [Required]
            [JsonPropertyName("jws")]
            public string Jws { get; set; }

            [JsonPropertyName("creator")]
            public string Creator { get; set; }

            [JsonPropertyName("domain")]
            public string Domain { get; set; }

            private readonly IHdCryptoService _cryptoService;

            public Proof(IHdCryptoService cryptoService)
            {
                _cryptoService = cryptoService;
            }

            public async Task<bool> SignAsync(string privateKeyIdentifier, string data)
            {
                try
                {
                    // Use HDCryptoService to sign the data
                    var signature = await _cryptoService.SignDataAsync(privateKeyIdentifier, data);
                    Jws = Convert.ToBase64String(signature);
                    Created = DateTime.UtcNow;
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            public async Task<bool> VerifyAsync(string publicKeyIdentifier, string data)
            {
                try
                {
                    var signature = Convert.FromBase64String(Jws);
                    return await _cryptoService.VerifyDataAsync(publicKeyIdentifier, data, signature);
                }
                catch
                {
                    return false;
                }
            }

            public bool IsValid()
            {
                return !string.IsNullOrEmpty(Type) &&
                       Created != default &&
                       !string.IsNullOrEmpty(VerificationMethod) &&
                       !string.IsNullOrEmpty(ProofPurpose) &&
                       !string.IsNullOrEmpty(Jws);
            }
        }


}