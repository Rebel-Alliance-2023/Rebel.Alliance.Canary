namespace Rebel.Alliance.Canary.Models;
public class VerifiableCredential
{
    public string? Id { get; set; }  // The unique identifier for the credential
    public string Issuer { get; set; }  // The entity that issued the credential
    public string Subject { get; set; }  // The subject (holder) of the credential
    public DateTime IssuanceDate { get; set; }  // The date the credential was issued
    public DateTime ExpirationDate { get; set; }  // The date the credential expires
    public Dictionary<string, string> Claims { get; set; }  // Claims contained in the credential (e.g., name, role, etc.)
    public Proof Proof { get; set; }  // The cryptographic proof of the credential (signature, etc.)

    public VerifiableCredential()
    {
        Claims = new Dictionary<string, string>();
        Proof = new Proof();
    }

    public bool IsExpired => DateTime.UtcNow > ExpirationDate;  // Property to check if the credential is expired

    public string? ParentCredentialId { get; internal set; }
}
