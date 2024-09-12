namespace Rebel.Alliance.Canary.Abstractions
{
    public interface IActorSystem
    {
        Task<IActorRef> CreateActorAsync<TActor>(string actorId) where TActor : IActor;
        Task<IActorRef> GetActorRefAsync(string actorId);
    }


}
