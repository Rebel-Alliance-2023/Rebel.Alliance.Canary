namespace Rebel.Alliance.Canary.Abstractions
{
    public interface IActorStateManager
    {
        Task<T?> TryGetStateAsync<T>(string stateName);
        Task<T> GetStateAsync<T>(string key);
        Task SetStateAsync<T>(string key, T value);
        Task ClearStateAsync();
    }


}
