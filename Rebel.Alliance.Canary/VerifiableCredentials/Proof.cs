namespace Rebel.Alliance.Canary.Models
{
    using System.ComponentModel.DataAnnotations;

    namespace Rebel.Alliance.Canary.Models
    {
        public class Proof
        {
            [Required]
            public string Type { get; set; } = "Ed25519Signature2018";

            [Required]
            public DateTime Created { get; set; }

            [Required]
            public string VerificationMethod { get; set; }

            [Required]
            public string ProofPurpose { get; set; } = "assertionMethod";

            [Required]
            public string Jws { get; set; }

            public string Creator { get; set; }

            public string Domain { get; set; }

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

}