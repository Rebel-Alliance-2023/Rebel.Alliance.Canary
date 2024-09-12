﻿using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Rebel.Alliance.Canary.InMemoryActorFramework;

namespace Rebel.Alliance.Canary.InMemoryActorFramework.Actors.OIDCClientActor
{
    public class OIDCClientActorHandler : IRequestHandler<ActorMessageEnvelope<OIDCClientActor>, object>
    {
        private readonly OIDCClientActor _actor;

        public OIDCClientActorHandler(OIDCClientActor actor)
        {
            _actor = actor;
        }

        public async Task<object> Handle(ActorMessageEnvelope<OIDCClientActor> request, CancellationToken cancellationToken)
        {
            return await _actor.ReceiveAsync((Abstractions.IActorMessage)request.Message);
        }
    }
}
