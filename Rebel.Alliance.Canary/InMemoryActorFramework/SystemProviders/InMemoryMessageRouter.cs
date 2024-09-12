using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Rebel.Alliance.Canary.Abstractions;

namespace Rebel.Alliance.Canary.InMemoryActorFramework.SystemProviders
{
    public class InMemoryMessageRouter : IRequestHandler<MessageRequest, Unit>
    {
        private readonly InMemoryActorSystem _actorSystem;

        public InMemoryMessageRouter(InMemoryActorSystem actorSystem)
        {
            _actorSystem = actorSystem;
        }

        public async Task<Unit> Handle(MessageRequest request, CancellationToken cancellationToken)
        {
            var actor = await _actorSystem.ActivateActorAsync<IActor>(request.ActorId);
            var method = actor.GetType().GetMethod(request.MethodName);
            if (method == null)
            {
                throw new InvalidOperationException($"The method {request.MethodName} does not exist on actor {actor.GetType().Name}.");
            }

            await (Task)method.Invoke(actor, request.Args);
            return Unit.Value;
        }
    }

    public class MessageRequest : IRequest<Unit>
    {
        public string ActorId { get; set; }
        public string MethodName { get; set; }
        public object[] Args { get; set; }

        public MessageRequest(string actorId, string methodName, object[] args)
        {
            ActorId = actorId;
            MethodName = methodName;
            Args = args;
        }
    }
}
