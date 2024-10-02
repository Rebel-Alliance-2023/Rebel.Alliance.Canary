using System;
using System.Reflection;
using System.Linq;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Rebel.Alliance.Canary.Actor.Interfaces;
using Rebel.Alliance.Canary.InMemoryActorFramework.ActorSystem;
using Rebel.Alliance.Canary.InMemoryActorFramework;
using MediatR.Pipeline;
using Rebel.Alliance.Canary.Security;
using Rebel.Alliance.Canary.VerifiableCredentials.Messaging;
using Rebel.Alliance.Canary.OIDC.Services;
using Rebel.Alliance.Canary.Actor.Interfaces.Actors;
using Rebel.Alliance.Canary.InMemoryActorFramework.Actors.TokenIssuerActor;
using Microsoft.Extensions.Logging;
using Rebel.Alliance.Canary.InMemoryActorFramework.Actors.CredentialVerifierActor;
using Rebel.Alliance.Canary.InMemoryActorFramework.Actors.RevocationManagerActor;
using Rebel.Alliance.Canary.InMemoryActorFramework.Actors.TrustFrameworkManagerActor;
using Rebel.Alliance.Canary.InMemoryActorFramework.Actors.VerifiableCredentialActor;
using Rebel.Alliance.Canary.InMemoryActorFramework.Actors.VerifiableCredentialAsRootOfTrustActor;
using Rebel.Alliance.Canary.VerifiableCredentials;
using Rebel.Alliance.Canary.InMemoryActorFramework.Actors.OIDCClientActor;

namespace Rebel.Alliance.Canary.Configuration
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCanaryActorSystem(
            this IServiceCollection services, 
            Action<IActorSystemConfiguration> configureAction)
        {
            services.AddLogging(builder => builder.AddConsole());
            var configuration = new ActorSystemConfiguration();
            configureAction(configuration);

            services.AddSingleton<IActorSystemConfiguration>(configuration);
            services.AddSingleton<IActorSystem>(sp => ActorSystemFactory.CreateActorSystem(sp, configuration));

            // Register MediatR
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
                typeof(ServiceCollectionExtensions).Assembly,  // Rebel.Alliance.Canary
                typeof(IActor).Assembly  // Rebel.Alliance.Canary.InMemoryActorFramework
            ));


            // Add MediatR pipeline behaviors
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestPreProcessorBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestPostProcessorBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

            // Register the InMemoryActorStateManager
            services.AddSingleton<IActorStateManager, InMemoryActorStateManager>();

            // Register all actor types
            RegisterActors(services, configuration.WebAppVc);
            RegisterMediatrRequests(services);

            // Register core services
            services.AddSingleton<IActorSystemProvider, InMemorySystemProvider>();
            services.AddSingleton<ICryptoService, CryptoService>();
            services.AddSingleton<IKeyManagementService, KeyManagementService>();
            services.AddSingleton<IKeyStore, InMemoryKeyStore>();
            services.AddSingleton<IActorMessageBus, InMemoryActorMessageBus>();

            services.AddSingleton<InMemoryActorSystem>();
            services.AddSingleton<IActorMessageBus, InMemoryActorMessageBus>();
            // Register OIDC services
            services.AddSingleton<DecentralizedOIDCProvider>();
            services.AddSingleton<OidcProviderService>(); //Do we need this one?

            services.AddTransient<IRequestHandler<MessageRequest, Unit>, InMemoryMessageRouter>();

            return services;
        }

        private static void RegisterActors(IServiceCollection services, VerifiableCredential webAppVc)
        {
            services.AddTransient<ITokenIssuerActor>(sp => new TokenIssuerActor(
                sp.GetRequiredService<ICryptoService>(),
                sp.GetRequiredService<IActorMessageBus>(),
                sp.GetRequiredService<IActorStateManager>(),
                sp.GetRequiredService<ILogger<TokenIssuerActor>>(),
                Guid.NewGuid().ToString()
            ));

            services.AddTransient<ICredentialVerifierActor>(sp => new CredentialVerifierActor(
                Guid.NewGuid().ToString(),
                sp.GetRequiredService<ICryptoService>(),
                sp.GetRequiredService<IRevocationManagerActor>(),
                sp.GetRequiredService<ILogger<CredentialVerifierActor>>(),
                webAppVc,
                sp.GetRequiredKeyedService<IKeyStore>("InMemoryKeyStore")
            ));


            services.AddTransient<IOIDCClientActor>(sp => new OIDCClientActor(
                Guid.NewGuid().ToString(),
                sp.GetRequiredService<IActorStateManager>(),
                sp.GetRequiredService<IActorMessageBus>(),
                sp.GetRequiredService<ILogger<OIDCClientActor>>()
            ));

            services.AddTransient<OIDCClientActor>(sp => (OIDCClientActor)sp.GetRequiredService<IOIDCClientActor>());

            services.AddTransient<IVerifiableCredentialActor>(sp => new VerifiableCredentialActor(
                Guid.NewGuid().ToString(),
                sp.GetRequiredService<ICryptoService>(),
                sp.GetRequiredService<ILogger<VerifiableCredentialActor>>(),
                sp.GetRequiredService<IActorStateManager>()
            ));

            services.AddTransient<IVerifiableCredentialAsRootOfTrustActor>(sp => new VerifiableCredentialAsRootOfTrustActor(
                Guid.NewGuid().ToString(),
                sp.GetRequiredService<ICryptoService>(),
                sp.GetRequiredService<IKeyManagementService>(),
                sp.GetRequiredService<ILogger<VerifiableCredentialAsRootOfTrustActor>>(),
                sp.GetRequiredService<IActorStateManager>()
            ));

            services.AddTransient<ITrustFrameworkManagerActor>(sp => new TrustFrameworkManagerActor(
                Guid.NewGuid().ToString(),
                sp.GetRequiredService<ILogger<TrustFrameworkManagerActor>>(),
                sp.GetRequiredService<IActorStateManager>()
            ));

            services.AddTransient<IRevocationManagerActor>(sp => new RevocationManagerActor(
                Guid.NewGuid().ToString(),
                sp.GetRequiredService<IActorMessageBus>(),
                sp.GetRequiredService<IActorStateManager>(),
                sp.GetRequiredService<ILogger<RevocationManagerActor>>()
            ));

            services.AddTransient<IRequestHandler<ActorMessageEnvelope<ITokenIssuerActor>, object>, ActorMessageHandler<ITokenIssuerActor>>();
        }


        private static void RegisterMediatrRequests(IServiceCollection services)
        {
            var canaryAssembly = typeof(ServiceCollectionExtensions).Assembly;
            var inMemoryActorAssembly = typeof(IActor).Assembly;

            var actorTypes = canaryAssembly.GetTypes()
                .Concat(inMemoryActorAssembly.GetTypes())
                .Where(t => typeof(IActor).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            foreach (var actorType in actorTypes)
            {
                // Register handler for concrete type
                RegisterHandler(services, actorType);

                // Register handler for the interface that this actor implements
                var interfaceType = actorType.GetInterfaces()
                    .FirstOrDefault(i => i != typeof(IActor) && typeof(IActor).IsAssignableFrom(i));
                if (interfaceType != null)
                {
                    RegisterHandler(services, interfaceType);
                }
            }
        }

        private static void RegisterHandler(IServiceCollection services, Type actorType)
        {
            var envelopeType = typeof(ActorMessageEnvelope<>).MakeGenericType(actorType);
            var handlerType = typeof(IRequestHandler<,>).MakeGenericType(envelopeType, typeof(object));
            var concreteHandlerType = typeof(ActorMessageHandler<>).MakeGenericType(actorType);
            services.AddTransient(handlerType, concreteHandlerType);
        }


    }

    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Handling {typeof(TRequest).Name}");
            var response = await next();
            Console.WriteLine($"Handled {typeof(TRequest).Name}");
            return response;
        }
    }
}