using System.Collections.Generic;
using Entitas;

namespace ECS.Systems.Navigation
{
    public class OnNewDestinationDoSetAgentDestination : ReactiveSystem<GameEntity>
    {
        private readonly INavigationService _navigationService;     

        public OnNewDestinationDoSetAgentDestination(Contexts contexts, INavigationService navigationService) : base(contexts.game)
        {
            _navigationService = navigationService;  
        }

        protected override ICollector<GameEntity> GetTrigger(IContext<GameEntity> context)
        {
            return context.CreateCollector(GameMatcher.Destination.Added());
        }

        protected override bool Filter(GameEntity entity)
        {
            return true;
        }

        protected override void Execute(List<GameEntity> entities)
        {
            foreach (var e in entities)
            {
                //TODO: Add PlayerControlledSystem to only iterate over game entities that are controlled by the player who sent the input
                _navigationService.SetAgentDestination(e.id.value, e.destination.value);   
            }
        }
    }
}
