namespace Rebel.Alliance.Canary.InMemoryActorFramework.ActorSystem
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;
    using MediatR;
    using Rebel.Alliance.Canary.Abstractions;

    public class InMemorySystemProvider : IActorSystemProvider
    {
        private readonly ConcurrentDictionary<string, object> _actors = new ConcurrentDictionary<string, object>();
        private readonly IActorStateManager _stateManager;
        private readonly IMediator _mediator;

        public InMemorySystemProvider(IActorStateManager stateManager, IMediator mediator)
        {
            _stateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public TActor GetActor<TActor>(string actorId) where TActor : class, IActor
        {
            var key = $"{typeof(TActor).Name}:{actorId}";

            if (_actors.TryGetValue(key, out var actor))
            {
                return actor as TActor;
            }

            return null;
        }

        public TActor CreateActor<TActor>(string actorId) where TActor : class, IActor, new()
        {
            var key = $"{typeof(TActor).Name}:{actorId}";

            var actor = new TActor();
            if (_actors.TryAdd(key, actor))
            {
                if (actor is IActor activatableActor)
                {
                    // Set necessary dependencies before activating the actor
                    activatableActor.SetActorStateManager(_stateManager);
                    activatableActor.SetMediator(_mediator);
                    activatableActor.OnActivateAsync().Wait(); // Initialize the actor
                }
                return actor;
            }

            return _actors[key] as TActor;
        }

        public async Task<bool> RemoveActor<TActor>(string actorId) where TActor : class, IActor
        {
            var key = $"{typeof(TActor).Name}:{actorId}";
            return _actors.TryRemove(key, out _);
        }

        public Task<IActorSystem> CreateActorSystem(string systemName)
        {
            return Task.FromResult<IActorSystem>(new InMemoryActorSystem(_stateManager, _mediator));
        }
    }
}
