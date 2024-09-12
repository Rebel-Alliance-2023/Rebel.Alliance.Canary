namespace Rebel.Alliance.Canary.Abstractions
{
    public interface IFrameworkActorAdapter
    {
        Task InitializeAsync(IActor actor);
        Task<object> InvokeMethodAsync(IActor actor, string methodName, object[] parameters);
    }


}
