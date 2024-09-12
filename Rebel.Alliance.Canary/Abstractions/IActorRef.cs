namespace Rebel.Alliance.Canary.Abstractions
{
    public interface IActorRef
    {
        string Id { get; }
        Task SendAsync(IActorMessage message);
    }


}
