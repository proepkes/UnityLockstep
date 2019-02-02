using System.Collections.Generic;
using System.Linq;
using Entitas;
using Lockstep.Core.Interfaces;

namespace Lockstep.Core.Systems.Navigation
{
    public class OnNavigationInputDoSetDestination : ReactiveSystem<InputEntity>
    {                                               
        private readonly INavigationService _navigationService;
        private readonly GameContext _contextsGame;

        public OnNavigationInputDoSetDestination(Contexts contexts, INavigationService navigationService) : base(contexts.input)
        {                                 
            _navigationService = navigationService;
            _contextsGame = contexts.game;
        }

        protected override ICollector<InputEntity> GetTrigger(IContext<InputEntity> context)
        {
            return context.CreateCollector(InputMatcher.AllOf(InputMatcher.Coordinate, InputMatcher.Selection, InputMatcher.ActorId));
        }

        protected override bool Filter(InputEntity entity)
        {
            return entity.hasCoordinate && entity.hasSelection && entity.hasActorId;
        }

        protected override void Execute(List<InputEntity> inputs)
        {     
            foreach (var input in inputs)
            {
                var destination = input.coordinate.value;

                var selectedEntities = _contextsGame
                    .GetEntities(GameMatcher.LocalId)
                    .Where(entity => 
                            input.selection.entityIds.Contains(entity.id.value) &&
                            entity.actorId.value == input.actorId.value);

                foreach (var entity in selectedEntities)
                {
                    entity.ReplaceDestination(destination);

                    //_navigationService.SetAgentDestination(entityId, destination);            
                }  
            }
        }
    }
}
