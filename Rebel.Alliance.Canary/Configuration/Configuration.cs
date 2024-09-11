using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Rebel.Alliance.Canary.Abstractions;
using Rebel.Alliance.Canary.Actors;
using Rebel.Alliance.Canary.SystemProviders;

namespace Rebel.Alliance.Canary.Configuration
{
    public interface IActorSystemConfiguration
    {
        string ActorSystemName { get; set; }
        string ActorFramework { get; set; }
        // Add other configuration properties as needed
    }

    public class ActorSystemConfiguration : IActorSystemConfiguration
    {
        public string ActorSystemName { get; set; }
        public string ActorFramework { get; set; }
    }

    public static class ActorSystemFactory
    {
        public static IActorSystem CreateActorSystem(IServiceProvider serviceProvider, IActorSystemConfiguration configuration)
        {
            var actorSystemProvider = serviceProvider.GetRequiredService<IActorSystemProvider>();
            return (IActorSystem)actorSystemProvider.CreateActorSystem(configuration.ActorSystemName);
        }
    }

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
    }

    // A simple in-memory implementation of IActorStateManager for demonstration purposes
    public class InMemoryActorStateManager : IActorStateManager
    {
        private readonly ConcurrentDictionary<string, object> _stateStore = new();
        public Task<T> GetStateAsync<T>(string key)
        {
            if (_stateStore.TryGetValue(key, out var value) && value is T typedValue)
            {
                return Task.FromResult(typedValue);
            }
            return Task.FromResult<T>(default);
        }

        public Task<T?> TryGetStateAsync<T>(string stateName)
        {
            if (_stateStore.TryGetValue(stateName, out var value) && value is T typedValue)
            {
                return Task.FromResult((T?)typedValue);
            }
            return Task.FromResult(default(T?));
        }


        public Task SetStateAsync<T>(string key, T value)
        {
            _stateStore[key] = value;
            return Task.CompletedTask;
        }

        public Task ClearStateAsync()
        {
            _stateStore.Clear();
            return Task.CompletedTask;
        }
    }
}
