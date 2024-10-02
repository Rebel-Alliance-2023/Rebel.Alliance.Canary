using Rebel.Alliance.Canary.Actor.Interfaces;

namespace Rebel.Alliance.Canary.VerifiableCredentials.Messaging
{
    public class CreateRootCredentialMessage : IActorMessage
    {
        public string IssuerId { get; }
        public Dictionary<string, string> Claims { get; }
        public string MasterKeyId { get; }

        public CreateRootCredentialMessage(string issuerId, Dictionary<string, string> claims, string masterKeyId)
        {
            IssuerId = issuerId;
            Claims = claims;
            MasterKeyId = masterKeyId;
        }

        public string MessageType => nameof(CreateRootCredentialMessage);
    }
}