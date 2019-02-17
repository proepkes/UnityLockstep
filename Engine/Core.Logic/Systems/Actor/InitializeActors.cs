using Entitas;

namespace Lockstep.Core.Logic.Systems.Actor
{
    public class InitializeActors : IInitializeSystem
    {
        private readonly ActorContext _actorContext;

        public InitializeActors(Contexts contexts)
        {
            _actorContext = contexts.actor;
        }
        public void Initialize()
        {
            foreach (var actor in _actorContext.GetEntities())
            {
                actor.AddEntityCount(0);
            }
        }
    }
}
