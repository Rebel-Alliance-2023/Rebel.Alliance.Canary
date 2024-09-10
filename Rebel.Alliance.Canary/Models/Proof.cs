namespace Rebel.Alliance.Canary.Models
{
    public class Proof
    {
        public string Type { get; set; } = "Ed25519Signature2018";  // The type of cryptographic signature (e.g., EdDSA, RSA, etc.)
        public DateTime Created { get; set; }  // The date and time when the proof was created
        public string VerificationMethod { get; set; }  // Reference to the verification method or public key
        public string ProofPurpose { get; set; } = "assertionMethod";  // The intended purpose of the proof (e.g., authentication, assertion, etc.)
        public string Jws { get; set; }  // The actual signature in JWS (JSON Web Signature) format
    }

}