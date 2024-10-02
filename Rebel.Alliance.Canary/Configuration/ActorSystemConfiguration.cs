using Rebel.Alliance.Canary.VerifiableCredentials;

namespace Rebel.Alliance.Canary.Configuration
{
    public class ActorSystemConfiguration : IActorSystemConfiguration
    {
        public string ActorSystemName { get; set; }
        public string ActorFramework { get; set; }
        public VerifiableCredential WebAppVc { get; set; }
    }
}
