using System.Collections.Generic;
using Entitas;

namespace ECS.Systems.Navigation
{
    public class OnNavigableEntityDoAddAgent : ReactiveSystem<GameEntity>
    {
        private readonly INavigationService _navigationService;

        public OnNavigableEntityDoAddAgent(Contexts contexts, INavigationService navigationService) : base(contexts.game)
        {
            _navigationService = navigationService;
        }

        protected override ICollector<GameEntity> GetTrigger(IContext<GameEntity> context)
        {
            return context.CreateCollector(GameMatcher.Navigable.Added());
        }

        protected override bool Filter(GameEntity entity)
        {
            return true;
        }

        protected override void Execute(List<GameEntity> entities)
        {
            foreach (var entity in entities)
            {
                _navigationService.AddAgent(entity.id.value, entity.position.value); 
            } 
        }
    }
}
