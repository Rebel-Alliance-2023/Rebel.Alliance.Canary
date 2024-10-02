namespace Rebel.Alliance.Canary.Actor.Interfaces
{
    public interface IActorSystemProvider
    {
        Task<IActorSystem> CreateActorSystem(string systemName);
    }


}
