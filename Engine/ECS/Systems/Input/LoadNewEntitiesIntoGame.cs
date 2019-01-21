using Entitas;

namespace ECS.Systems.Input
{
    public class LoadNewEntitiesIntoGame : IExecuteSystem, ICleanupSystem
    {
        private readonly IGroup<GameEntity> _pendingEntities;
        private readonly IGameService _configurationService;

        public LoadNewEntitiesIntoGame(Contexts contexts, IGameService configurationService)
        {
            _configurationService = configurationService;
            
            _pendingEntities = contexts.game.GetGroup(GameMatcher.ConfigId);
        }
        public void Execute()
        {
            foreach (var entity in _pendingEntities.GetEntities())
            {
                _configurationService.ApplyEntity(entity, entity.configId.value);
            }
        }

        public void Cleanup()
        {
            foreach (var entity in _pendingEntities.GetEntities())
            {
                entity.RemoveConfigId();
            }
        }
    }
}
