using MediatR;
using System;
using System.Threading.Tasks;

namespace Rebel.Alliance.Canary.Abstractions
{

    public abstract class ActorBase : IActor
    {
        protected IActorStateManager StateManager { get; private set; }
        protected IMediator Mediator { get; private set; }

        public string Id { get; private set; }

        public ActorBase(string id)
        {
            Id = id;
        }

        public virtual Task OnActivateAsync()
        {
            // Default implementation for actor activation
            return Task.CompletedTask;
        }

        public virtual Task OnDeactivateAsync()
        {
            // Default implementation for actor deactivation
            return Task.CompletedTask;
        }

        public void SetActorStateManager(IActorStateManager stateManager)
        {
            StateManager = stateManager;
        }

        public void SetMediator(IMediator mediator)
        {
            Mediator = mediator;
        }

        public virtual async Task<object> ReceiveAsync(IActorMessage message)
        {
            // Default implementation for handling messages
            return Task.CompletedTask;
        }
    }


}
