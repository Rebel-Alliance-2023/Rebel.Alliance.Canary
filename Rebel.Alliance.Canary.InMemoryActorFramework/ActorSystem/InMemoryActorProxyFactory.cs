namespace Rebel.Alliance.Canary.InMemoryActorFramework.ActorSystem
{
    using System;
    using Rebel.Alliance.Canary.Actor.Interfaces;

    public class InMemoryActorProxyFactory : IActorProxyFactory
    {
        private readonly IActorSystem _actorSystem;

        public InMemoryActorProxyFactory(IActorSystem actorSystem)
        {
            _actorSystem = actorSystem;
        }

        public TActorInterface CreateActorProxy<TActorInterface>(string actorId) where TActorInterface : IActor
        {
            if (string.IsNullOrEmpty(actorId))
            {
                throw new ArgumentException("ActorId cannot be null or empty", nameof(actorId));
            }

            var actorRef = _actorSystem.GetActorRefAsync(actorId).Result;
            if (actorRef == null)
            {
                throw new InvalidOperationException($"No actor found with ID {actorId}");
            }

            // Creates a dynamic proxy that wraps the in-memory actor reference
            return (TActorInterface)actorRef;
        }
    }

    public interface IActorProxyFactory
    {
    }
}
