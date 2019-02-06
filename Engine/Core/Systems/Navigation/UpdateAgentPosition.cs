using System.Collections.Generic;
using System.Linq;
using BEPUutilities;
using Entitas;
using Lockstep.Core.Services;

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
            var updatedPositions = new Dictionary<uint, Vector2>();

            foreach (var entity in _gameContext.GetEntities().Where(e => e.hasId))
            {
                if (entity.velocity.value == Vector2.Zero)
                    continue;
                entity.ReplacePosition(entity.position.value + entity.velocity.value);

                updatedPositions.Add(entity.localId.value, entity.position.value);
            } 
            
            _navigationService.SetAgentPositions(updatedPositions);
        }
    }   
}
