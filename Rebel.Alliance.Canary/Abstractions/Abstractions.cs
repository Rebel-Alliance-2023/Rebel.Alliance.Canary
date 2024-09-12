using MediatR;
using System;
using System.Threading.Tasks;

namespace Rebel.Alliance.Canary.Abstractions
{
    public interface IActor
    {
        string Id { get; }
        Task OnActivateAsync();
        Task OnDeactivateAsync();
        void SetActorStateManager(IActorStateManager stateManager);
        void SetMediator(IMediator mediator);
        Task<object> ReceiveAsync(IActorMessage message);
    }


    public interface IActorMessage
    {
        string MessageType { get; }
    }

    public interface IActorRef
    {
        string Id { get; }
        Task SendAsync(IActorMessage message);
    }

    public interface IActorSystem
    {
        Task<IActorRef> CreateActorAsync<TActor>(string actorId) where TActor : IActor;
        Task<IActorRef> GetActorRefAsync(string actorId);
    }

    public interface IActorStateManager
    {
        Task<T?> TryGetStateAsync<T>(string stateName);
        Task<T> GetStateAsync<T>(string key);
        Task SetStateAsync<T>(string key, T value);
        Task ClearStateAsync();
    }

    public interface IActorSystemProvider
    {
        Task<IActorSystem> CreateActorSystem(string systemName);
    }

    public interface IFrameworkActorAdapter
    {
        Task InitializeAsync(IActor actor);
        Task<object> InvokeMethodAsync(IActor actor, string methodName, object[] parameters);
    }

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
