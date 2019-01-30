using System.Collections.Generic;
using Entitas;
using Lockstep.Core.Interfaces;

namespace Lockstep.Core.Systems.Navigation
{
    public class OnNavigationInputDoSetDestination : ReactiveSystem<InputEntity>
    {                                               
        private readonly INavigationService _navigationService;
        private GameContext _contextsGame;

        public OnNavigationInputDoSetDestination(Contexts contexts, INavigationService navigationService) : base(contexts.input)
        {                                 
            _navigationService = navigationService;
            _contextsGame = contexts.game;
        }

        protected override ICollector<InputEntity> GetTrigger(IContext<InputEntity> context)
        {
            return context.CreateCollector(InputMatcher.AllOf(InputMatcher.Coordinate, InputMatcher.EntityIds));
        }

        protected override bool Filter(InputEntity entity)
        {
            return entity.hasCoordinate && entity.hasEntityIds;
        }

        protected override void Execute(List<InputEntity> inputs)
        {     
            foreach (var input in inputs)
            {
                var destination = input.coordinate.value;
                foreach (var entityId in input.entityIds.values)
                {
                    _contextsGame.GetEntityWithId(entityId).ReplaceDestination(destination);
                    //_navigationService.SetAgentDestination(entityId, destination);            
                }  
            }
        }
    }
}
