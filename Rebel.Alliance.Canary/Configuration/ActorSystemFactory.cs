using Microsoft.Extensions.DependencyInjection;
using Rebel.Alliance.Canary.Abstractions;

namespace Rebel.Alliance.Canary.Configuration
{
    public static class ActorSystemFactory
    {
        public static IActorSystem CreateActorSystem(IServiceProvider serviceProvider, IActorSystemConfiguration configuration)
        {
            var actorSystemProvider = serviceProvider.GetRequiredService<IActorSystemProvider>();
            return (IActorSystem)actorSystemProvider.CreateActorSystem(configuration.ActorSystemName);
        }
    }
}
