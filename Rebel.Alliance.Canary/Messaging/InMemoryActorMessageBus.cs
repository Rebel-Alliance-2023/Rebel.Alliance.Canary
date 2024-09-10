using MediatR;
using Rebel.Alliance.Canary.Abstractions;
using System;
using System.Threading.Tasks;

namespace Rebel.Alliance.Canary.Messaging
{
    public class InMemoryActorMessageBus : IActorMessageBus
    {
        private readonly IMediator _mediator;

        public InMemoryActorMessageBus(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task SendMessageAsync<TActor>(string actorId, object message) where TActor : IActor
        {
            // Using Mediatr to send a message asynchronously to a specific actor
            await _mediator.Send(new ActorMessageEnvelope<TActor>(actorId, message));
        }

        public async Task<TResult> SendMessageAsync<TActor, TResult>(string actorId, object message) where TActor : IActor
        {
            var envelope = new ActorMessageEnvelope<TActor>(actorId, message);
            // Using Mediatr to send a message asynchronously to a specific actor and return a result
            return await _mediator.Send<TResult>((IRequest<TResult>)envelope);
        }

        public async Task RegisterHandlerAsync<TMessage>(Func<TMessage, Task> handler)
        {
            // Register a handler with Mediatr
            _mediator.Publish(new RegisterMessageHandlerRequest<TMessage>(handler));
        }

        public async Task DispatchMessageAsync(object message)
        {
            // Dispatch the message using Mediatr
            await _mediator.Publish(message);
        }
    }

    // Supporting classes for Mediatr message envelope and handler registration

    public class ActorMessageEnvelope<TActor> : IRequest where TActor : IActor
    {
        public string ActorId { get; }
        public object Message { get; }

        public ActorMessageEnvelope(string actorId, object message)
        {
            ActorId = actorId;
            Message = message;
        }
    }

    public class RegisterMessageHandlerRequest<TMessage> : IRequest
    {
        public Func<TMessage, Task> Handler { get; }

        public RegisterMessageHandlerRequest(Func<TMessage, Task> handler)
        {
            Handler = handler;
        }
    }
}
