using System.Collections.Generic;
using Rebel.Alliance.Canary.Actor.Interfaces;

namespace Rebel.Alliance.Canary.VerifiableCredentials.Messaging
{
    public class IssueCredentialMessage : IActorMessage
    {
        public string IssuerId { get; }
        public string SubjectId { get; }
        public DateTime ExpirationDate { get; }
        public Dictionary<string, string> Claims { get; }

        public IssueCredentialMessage(string issuerId, string subjectId, DateTime expirationDate, Dictionary<string, string> claims)
        {
            IssuerId = issuerId ?? throw new ArgumentNullException(nameof(issuerId));
            SubjectId = subjectId ?? throw new ArgumentNullException(nameof(subjectId));
            ExpirationDate = expirationDate;
            Claims = claims ?? throw new ArgumentNullException(nameof(claims));
        }

        public string MessageType => nameof(IssueCredentialMessage);
    }
}
