using Entitas;
using Lockstep.Common;
using Lockstep.Core.Logic.Interfaces.Services;

namespace Lockstep.Core.Logic.Systems.Game.Navigation
{
    public class SyncAgentVelocity : IExecuteSystem
    {                                                       
        private readonly GameContext _gameContext;
        private readonly INavigationService _navigationService;

        public SyncAgentVelocity(Contexts contexts, ServiceContainer services)
        {
            _gameContext = contexts.game;
            _navigationService = services.Get<INavigationService>();
        }

        public void Execute()
        {                                
            foreach (var (entityLocalId, velocity) in _navigationService.GetAgentVelocities())
            {
                var gameEntity = _gameContext.GetEntityWithLocalId(entityLocalId);
                if (gameEntity.velocity.value != velocity)
                {
                    gameEntity.ReplaceVelocity(velocity);
                }
            }
        }      
    }
}     
