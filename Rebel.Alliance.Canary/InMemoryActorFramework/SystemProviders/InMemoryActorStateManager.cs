using System.Collections.Concurrent;
using System.Threading.Tasks;
using Rebel.Alliance.Canary.Abstractions;

namespace Rebel.Alliance.Canary.InMemoryActorFramework.SystemProviders
{
    public class InMemoryActorStateManager : IActorStateManager
    {
        // A thread-safe dictionary to store actor states
        private readonly ConcurrentDictionary<string, object> _stateStore = new ConcurrentDictionary<string, object>();

        public Task SetStateAsync<T>(string stateName, T value)
        {
            _stateStore[stateName] = value;
            return Task.CompletedTask;
        }

        public Task<T> GetStateAsync<T>(string stateName)
        {
            if (_stateStore.TryGetValue(stateName, out var value) && value is T typedValue)
            {
                return Task.FromResult(typedValue);
            }

            throw new KeyNotFoundException($"State with name '{stateName}' not found.");
        }

        public Task<T?> TryGetStateAsync<T>(string stateName)
        {
            if (_stateStore.TryGetValue(stateName, out var value) && value is T typedValue)
            {
                return Task.FromResult((T?)typedValue);
            }

            return Task.FromResult(default(T?));
        }

        public Task RemoveStateAsync(string stateName)
        {
            _stateStore.TryRemove(stateName, out _);
            return Task.CompletedTask;
        }

        public Task ClearStateAsync()
        {
            _stateStore.Clear();
            return Task.CompletedTask;
        }
    }
}
