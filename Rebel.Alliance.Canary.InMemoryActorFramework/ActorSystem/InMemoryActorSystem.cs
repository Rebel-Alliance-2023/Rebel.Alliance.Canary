using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using MediatR;
using Rebel.Alliance.Canary.Actor.Interfaces;

namespace Rebel.Alliance.Canary.InMemoryActorFramework.ActorSystem
{
    public class InMemoryActorSystem : IActorSystem
    {
        private readonly IActorStateManager _stateManager;
        private readonly IMediator _mediator;
        private readonly ConcurrentDictionary<string, IActor> _actors = new();

        public InMemoryActorSystem(IActorStateManager stateManager, IMediator mediator)
        {
            _stateManager = stateManager;
            _mediator = mediator;
        }

        // Updated to remove `new()` constraint
        public async Task<IActorRef> CreateActorAsync<TActor>(string actorId) where TActor : IActor
        {
            var actor = await ActivateActorAsync<TActor>(actorId);
            return new InMemoryActorRef<TActor>(actor, actorId);
        }

        public async Task<IActorRef> GetActorRefAsync(string actorId)
        {
            if (_actors.TryGetValue(actorId, out var actor))
            {
                return new InMemoryActorRef<IActor>(actor, actorId);
            }
            return null;
        }

        public async Task<TActor> ActivateActorAsync<TActor>(string actorId) where TActor : IActor
        {
            if (_actors.TryGetValue(actorId, out var existingActor))
            {
                return (TActor)existingActor;
            }

            var actor = (TActor)Activator.CreateInstance(typeof(TActor)); // Use Activator to create an instance
            actor.SetActorStateManager(_stateManager);
            actor.SetMediator(_mediator);
            await actor.OnActivateAsync();
            _actors[actorId] = actor;
            return actor;
        }

        public async Task DeactivateActorAsync(string actorId)
        {
            if (_actors.TryRemove(actorId, out var actor))
            {
                await actor.OnDeactivateAsync();
            }
        }

        public async Task SendMessageAsync<TActor>(string actorId, INotification notification) where TActor : IActor
        {
            var actor = await ActivateActorAsync<TActor>(actorId);
            await _mediator.Publish(notification);
        }
    }


    public class InMemoryActorRef<TActor> : IActorRef where TActor : IActor
    {
        public TActor Actor { get; }
        public string ActorId { get; }

        public InMemoryActorRef(TActor actor, string actorId)
        {
            Actor = actor;
            ActorId = actorId;
        }

        public InMemoryActorRef()
        {
        }

        public string Id => ActorId;

        public Task SendAsync(IActorMessage message)
        {
            return Actor.ReceiveAsync(message);
        }
    }

}
