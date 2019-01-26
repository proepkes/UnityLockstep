using BEPUutilities;
using Entitas;
using Lockstep.Core.Interfaces;

namespace Lockstep.Core.Systems.Navigation
{
    public class SyncAgentVelocity : IExecuteSystem
    {                                                       
        private readonly INavigationService _navigationService;
        private readonly GameContext _gameContext;

        public SyncAgentVelocity(Contexts contexts, INavigationService navigationService)
        {
            _navigationService = navigationService;
            _gameContext = contexts.game;
        }

        public void Execute()
        {
            var agentVelocities = _navigationService.GetAgentVelocities();
            foreach (var velocity in agentVelocities)
            {
                var gameEntity = _gameContext.GetEntityWithId(velocity.Key);
                //if (velocity.Value != Vector2.Zero)
                {
                    gameEntity.ReplaceVelocity(velocity.Value);
                }
            }                                   
        }      
    }
}     
