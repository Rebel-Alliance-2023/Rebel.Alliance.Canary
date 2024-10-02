namespace Rebel.Alliance.Canary.Actor.Interfaces
{
    public interface IActorSystem
    {
        Task<IActorRef> CreateActorAsync<TActor>(string actorId) where TActor : IActor;
        Task<IActorRef> GetActorRefAsync(string actorId);
    }


}
