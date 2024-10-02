using System;
using System.Collections.Concurrent;
using Rebel.Alliance.Canary.Actor.Interfaces;

namespace Rebel.Alliance.Canary.Configuration
{

    // A simple in-memory implementation of IActorStateManager for demonstration purposes
    public class InMemoryActorStateManager : IActorStateManager
    {
        private readonly ConcurrentDictionary<string, object> _stateStore = new();
        public Task<T> GetStateAsync<T>(string key)
        {
            if (_stateStore.TryGetValue(key, out var value) && value is T typedValue)
            {
                return Task.FromResult(typedValue);
            }
            return Task.FromResult<T>(default);
        }

        public Task<T?> TryGetStateAsync<T>(string stateName)
        {
            if (_stateStore.TryGetValue(stateName, out var value) && value is T typedValue)
            {
                return Task.FromResult((T?)typedValue);
            }
            return Task.FromResult(default(T?));
        }


        public Task SetStateAsync<T>(string key, T value)
        {
            _stateStore[key] = value;
            return Task.CompletedTask;
        }

        public Task ClearStateAsync()
        {
            _stateStore.Clear();
            return Task.CompletedTask;
        }
    }
}
