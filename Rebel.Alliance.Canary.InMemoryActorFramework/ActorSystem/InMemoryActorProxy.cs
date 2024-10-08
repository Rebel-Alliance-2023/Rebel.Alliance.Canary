﻿namespace Rebel.Alliance.Canary.InMemoryActorFramework.ActorSystem
{
    using System.Threading.Tasks;
    using Rebel.Alliance.Canary.Actor.Interfaces;

    public class InMemoryActorProxy<TActor> : IActorRef where TActor : IActor
    {
        private readonly TActor _actor;
        private readonly string _actorId;

        public InMemoryActorProxy(TActor actor, string actorId)
        {
            _actor = actor;
            _actorId = actorId;
        }

        public string Id => _actorId;

        public Task SendAsync(IActorMessage message)
        {
            return _actor.ReceiveAsync(message);
        }
    }
}
