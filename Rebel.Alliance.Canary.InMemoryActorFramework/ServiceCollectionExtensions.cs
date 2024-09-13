using Microsoft.Extensions.DependencyInjection;
using Rebel.Alliance.Canary.Actor.Interfaces;
using Rebel.Alliance.Canary.InMemoryActorFramework.Actors;
using Rebel.Alliance.Canary.InMemoryActorFramework.ActorSystem;
using Rebel.Alliance.Canary.OIDC.Services;
using Rebel.Alliance.Canary.Security;

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

            // Register core services
            services.AddSingleton<IActorSystemProvider, InMemorySystemProvider>();
            services.AddSingleton<ICryptoService, CryptoService>();
            services.AddSingleton<IKeyManagementService, KeyManagementService>();
            services.AddSingleton<IKeyStore, InMemoryKeyStore>();
            services.AddSingleton<IActorMessageBus, InMemoryActorMessageBus>();

            // Register OIDC services
            services.AddSingleton<DecentralizedOIDCProvider>();
            services.AddSingleton<OidcProviderService>();

            return services;
        }
    }
}
