using Entitas;
using Simulation.Behaviour.Services;

namespace Simulation.Behaviour.Game.Navigation
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
            //var agentVelocities = _navigationService.GetAgentVelocities();
            //foreach (var velocity in agentVelocities)
            //{
            //    var gameEntity = _gameContext.GetEntityWithId(velocity.Key);
            //    if (gameEntity.velocity.value != velocity.Value)
            //    {
            //        gameEntity.ReplaceVelocity(velocity.Value);
            //    }
            //}                                   
        }      
    }
}     
