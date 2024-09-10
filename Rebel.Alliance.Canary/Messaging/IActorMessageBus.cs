using Rebel.Alliance.Canary.Abstractions;

namespace Rebel.Alliance.Canary.Messaging
{
    public interface IActorMessageBus
    {
        /// <summary>
        /// Sends a message to a specific actor.
        /// </summary>
        /// <typeparam name="TActor">The type of the actor receiving the message.</typeparam>
        /// <param name="actorId">The unique identifier of the actor.</param>
        /// <param name="message">The message to be sent.</param>
        Task<TResult> SendMessageAsync<TActor, TResult>(string actorId, object message)
            where TActor : IActor;

        Task SendMessageAsync<TActor>(string actorId, object message)
            where TActor : IActor;

        /// <summary>
        /// Registers a handler for a specific type of message.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message to handle.</typeparam>
        /// <param name="handler">The function that handles the message.</param>
        Task RegisterHandlerAsync<TMessage>(Func<TMessage, Task> handler);

        /// <summary>
        /// Handles message dispatching within the system.
        /// </summary>
        Task DispatchMessageAsync(object message);
    }
}
