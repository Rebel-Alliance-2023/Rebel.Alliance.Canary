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

            // Register MediatR
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly));

            // Add MediatR pipeline behaviors
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestPreProcessorBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestPostProcessorBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

            // Register the InMemoryActorStateManager
            services.AddSingleton<IActorStateManager, InMemoryActorStateManager>();

            // Register all actor types
            RegisterActors(services);

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

        private static void RegisterActors(IServiceCollection services)
        {
            var actorTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => typeof(IActor).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            foreach (var actorType in actorTypes)
            {
                services.AddTransient(actorType);

                // Create the generic ActorMessageEnvelope type
                var envelopeType = typeof(ActorMessageEnvelope<>).MakeGenericType(actorType);

                // Create the generic IRequestHandler type
                var handlerType = typeof(IRequestHandler<,>).MakeGenericType(envelopeType, typeof(object));

                // Create the generic ActorMessageHandler type
                var concreteHandlerType = typeof(ActorMessageHandler<>).MakeGenericType(actorType);

                // Register the handler
                services.AddTransient(handlerType, concreteHandlerType);
            }

            services.AddTransient<ITokenIssuerActor, TokenIssuerActor>(sp => new TokenIssuerActor(
               sp.GetRequiredService<ICryptoService>(),
               sp.GetRequiredService<IActorMessageBus>(),
               sp.GetRequiredService<IActorStateManager>(),
               Guid.NewGuid().ToString()
            ));

            //// Ensure ITokenIssuerActor and its handler are registered
            //services.AddTransient<ITokenIssuerActor, TokenIssuerActor>(sp => new TokenIssuerActor("YourStringParameter"));


            services.AddTransient<IRequestHandler<ActorMessageEnvelope<ITokenIssuerActor>, object>, ActorMessageHandler<ITokenIssuerActor>>();
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