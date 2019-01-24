using ECS;
using Lockstep.Core.Features;
using Lockstep.Core.Interfaces;

namespace Lockstep.Core
{
    public sealed class LockstepSystems : Entitas.Systems, ISystems
    {
        private readonly Contexts _contexts; 
        
        public IFrameDataSource DataSource { get; }

        public long HashCode => _contexts.gameState.hashCode.value;

        public LockstepSystems(Contexts contexts, ServiceContainer serviceContainer, IFrameDataSource dataSource)
        {
            DataSource = dataSource;
            _contexts = contexts;
            contexts.game.OnEntityCreated += (context, entity) => ((GameEntity) entity).AddId(entity.creationIndex);

            Add(new InputFeature(contexts, serviceContainer, DataSource));

            Add(new NavigationFeature(contexts, serviceContainer));

            Add(new GameEventSystems(contexts));

            Add(new HashCodeFeature(contexts, serviceContainer));
        }


        public void Tick()
        {
            Execute();
            Cleanup();
        }
    }
}     