namespace Rebel.Alliance.Canary.InMemoryActorFramework.ActorSystem
{
    using System.Threading.Tasks;
    using Rebel.Alliance.Canary.Actor.Interfaces;

    public class InMemoryActorSystemRef<TActor> : IActorRef where TActor : IActor
    {
        private readonly IActorSystem _actorSystem;
        private readonly string _actorId;

        public InMemoryActorSystemRef(IActorSystem actorSystem, string actorId)
        {
            _actorSystem = actorSystem;
            _actorId = actorId;
        }

        public string Id => _actorId;

        public async Task SendAsync(IActorMessage message)
        {
            var actorRef = await _actorSystem.GetActorRefAsync(_actorId);
            await actorRef.SendAsync(message);
        }
    }
}
