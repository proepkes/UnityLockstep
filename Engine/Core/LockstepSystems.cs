using ECS;
using ECS.Features;

namespace Lockstep.Framework
{
    public sealed class LockstepSystems : Entitas.Systems
    {
        private readonly Contexts _contexts;

        public long HashCode => _contexts.gameState.hashCode.value;

        public LockstepSystems(Contexts contexts, ServiceContainer serviceContainer)
        {
            _contexts = contexts;
            contexts.game.OnEntityCreated += (context, entity) => ((GameEntity) entity).AddId(entity.creationIndex);

            Add(new InputFeature(contexts, serviceContainer));

            Add(new NavigationFeature(contexts, serviceContainer));

            Add(new GameEventSystems(contexts));

            Add(new HashCodeFeature(contexts, serviceContainer));
        }

        public void Simulate()
        {                                             
            Execute();
            Cleanup();                   
        }
    }
}     