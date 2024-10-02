using Rebel.Alliance.Canary.VerifiableCredentials;

namespace Rebel.Alliance.Canary.Configuration
{
    public interface IActorSystemConfiguration
    {
        string ActorSystemName { get; set; }
        string ActorFramework { get; set; }
        VerifiableCredential WebAppVc { get; set; }
        // Add other configuration properties as needed
    }
}
