using Microsoft.Extensions.DependencyInjection;
using Rebel.Alliance.Canary.Abstractions;
using Rebel.Alliance.Canary.InMemoryActorFramework.Actors.CredentialHolderActor;
using Rebel.Alliance.Canary.InMemoryActorFramework.Actors.CredentialIssuerActor;
using Rebel.Alliance.Canary.InMemoryActorFramework.Actors.CredentialVerifierActor;
using Rebel.Alliance.Canary.InMemoryActorFramework.Actors.OIDCClientActor;
using Rebel.Alliance.Canary.InMemoryActorFramework.Actors.RevocationManagerActor;
using Rebel.Alliance.Canary.InMemoryActorFramework.Actors.TokenIssuerActor;
using Rebel.Alliance.Canary.InMemoryActorFramework.Actors.TrustFrameworkManagerActor;
using Rebel.Alliance.Canary.InMemoryActorFramework.Actors.VerifiableCredentialActor;
using Rebel.Alliance.Canary.InMemoryActorFramework.Actors.VerifiableCredentialAsRootOfTrustActor;
using Rebel.Alliance.Canary.InMemoryActorFramework.ActorSystem;

namespace Rebel.Alliance.Canary.Configuration
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCanaryActorSystem(this IServiceCollection services, Action<IActorSystemConfiguration> configureAction)
        {
            var configuration = new ActorSystemConfiguration();
            configureAction(configuration);

            services.AddSingleton<IActorSystemConfiguration>(configuration);
            services.AddSingleton<IActorSystem>(sp => ActorSystemFactory.CreateActorSystem(sp, configuration));

            // Register the InMemoryActorStateManager
            services.AddSingleton<IActorStateManager, InMemoryActorStateManager>();

            // Register all actor types
            services.AddTransient<VerifiableCredentialActor>();
            services.AddTransient<CredentialIssuerActor>();
            services.AddTransient<CredentialVerifierActor>();
            services.AddTransient<CredentialHolderActor>();
            services.AddTransient<RevocationManagerActor>();
            services.AddTransient<TrustFrameworkManagerActor>();
            services.AddTransient<VerifiableCredentialAsRootOfTrustActor>();
            services.AddTransient<OIDCClientActor>();
            services.AddTransient<TokenIssuerActor>();

            services.AddSingleton<IActorSystemProvider, InMemorySystemProvider>();
            return services;
        }
    }
}

/*
NOTE: In a different approach we might create a "sink" for a variety of 3rd Party Actor Frameworks, and then register the appropriate IActorSystemProvider based on the configuration. This would allow us to support multiple actor frameworks in the same application. Like so:


        public static IServiceCollection AddCanaryActorSystem(this IServiceCollection services, Action<IActorSystemConfiguration> configureAction)
        {
            var configuration = new ActorSystemConfiguration();
            configureAction(configuration);

            services.AddSingleton<IActorSystemConfiguration>(configuration);
            services.AddSingleton<IActorSystem>(sp => ActorSystemFactory.CreateActorSystem(sp, configuration));

            // Register the InMemoryActorStateManager
            services.AddSingleton<IActorStateManager, InMemoryActorStateManager>();

            // Register all actor types
            services.AddTransient<VerifiableCredentialActor>();
            services.AddTransient<CredentialIssuerActor>();
            services.AddTransient<CredentialVerifierActor>();
            services.AddTransient<CredentialHolderActor>();
            services.AddTransient<RevocationManagerActor>();
            services.AddTransient<TrustFrameworkManagerActor>();
            services.AddTransient<VerifiableCredentialAsRootOfTrustActor>();
            services.AddTransient<OIDCClientActor>();
            services.AddTransient<TokenIssuerActor>();

            // Depending on the configured framework, register the appropriate IActorSystemProvider
            switch (configuration.ActorFramework.ToLowerInvariant())
            {
                case "in-memory":
                    services.AddSingleton<IActorSystemProvider, InMemorySystemProvider>();
                    break;
                case "orleans":
                    // services.AddSingleton<IActorSystemProvider, OrleansActorSystemProvider>();
                    break;
                case "akka":
                    // services.AddSingleton<IActorSystemProvider, AkkaActorSystemProvider>();
                    break;
                case "protoactor":
                    // services.AddSingleton<IActorSystemProvider, ProtoActorSystemProvider>();
                    break;
                // Add cases for other supported frameworks
                default:
                    throw new NotSupportedException($"Actor framework '{configuration.ActorFramework}' is not supported.");
            }

            return services;
        }

But it is unlikely that we would need to support multiple actor frameworks in the same application, so we will keep it simple for now.


*/