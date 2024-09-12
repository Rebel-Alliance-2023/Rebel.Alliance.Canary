namespace Rebel.Alliance.Canary.Abstractions
{
    public interface IActorSystemProvider
    {
        Task<IActorSystem> CreateActorSystem(string systemName);
    }


}
