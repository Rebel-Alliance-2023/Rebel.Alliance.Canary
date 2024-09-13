namespace Rebel.Alliance.Canary.Actor.Interfaces
{
    public interface IActorRef
    {
        string Id { get; }
        Task SendAsync(IActorMessage message);
    }


}
