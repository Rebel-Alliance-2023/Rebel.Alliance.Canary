namespace Rebel.Alliance.Canary.Actor.Interfaces
{
    public interface IFrameworkActorAdapter
    {
        Task InitializeAsync(IActor actor);
        Task<object> InvokeMethodAsync(IActor actor, string methodName, object[] parameters);
    }


}
