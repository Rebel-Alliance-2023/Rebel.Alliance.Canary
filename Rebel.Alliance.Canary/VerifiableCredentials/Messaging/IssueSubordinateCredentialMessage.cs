using Rebel.Alliance.Canary.Actor.Interfaces;

namespace Rebel.Alliance.Canary.VerifiableCredentials.Messaging
{
    public class IssueSubordinateCredentialMessage : IActorMessage
    {
        public string IssuerId { get; }
        public VerifiableCredential RootCredential { get; }
        public Dictionary<string, string> Claims { get; }
        public string DerivedKeyId { get; }

        public IssueSubordinateCredentialMessage(string issuerId, VerifiableCredential rootCredential, Dictionary<string, string> claims, string derivedKeyId)
        {
            IssuerId = issuerId;
            RootCredential = rootCredential;
            Claims = claims;
            DerivedKeyId = derivedKeyId;
        }

        public string MessageType => nameof(IssueSubordinateCredentialMessage);
    }
}