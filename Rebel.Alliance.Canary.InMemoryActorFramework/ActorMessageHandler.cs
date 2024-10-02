using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Rebel.Alliance.Canary.Actor.Interfaces;

namespace Rebel.Alliance.Canary.InMemoryActorFramework;
public class ActorMessageHandler<TActor> : IRequestHandler<ActorMessageEnvelope<TActor>, object> where TActor : IActor
{
    private readonly IServiceProvider _serviceProvider;

    public ActorMessageHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<object> Handle(ActorMessageEnvelope<TActor> request, CancellationToken cancellationToken)
    {
        var actor = _serviceProvider.GetRequiredService<TActor>();
        return await actor.ReceiveAsync((IActorMessage)request.Message);
    }
}