using MediatR;

namespace Rebel.Alliance.Canary.Actor.Interfaces
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


}
