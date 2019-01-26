using System.Collections.Generic;
using BEPUutilities;
using Entitas;
using Lockstep.Core.Interfaces;

namespace Lockstep.Core.Systems.Navigation
{
    public class UpdateAgentPosition : IExecuteSystem
    {
        private readonly INavigationService _navigationService;
        private readonly GameContext _gameContext;

        public UpdateAgentPosition(Contexts contexts, INavigationService navigationService)
        {
            _navigationService = navigationService;
            _gameContext = contexts.game;                                                                       
        }

        public void Execute()
        {
            var updatedPositions = new Dictionary<int, Vector2>();

            foreach (var entity in _gameContext.GetEntities())
            {
                if (entity.velocity.value != Vector2.Zero)
                {
                    entity.ReplacePosition(entity.position.value += entity.velocity.value);

                    updatedPositions.Add(entity.id.value, entity.position.value);
                }
            } 
            
            _navigationService.SetAgentPositions(updatedPositions);
        }
    }
}
