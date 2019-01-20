using System.Collections.Generic;
using Entitas;

namespace ECS.Systems.Navigation
{
    public class OnNavigationEntityDoAddAgent : ReactiveSystem<GameEntity>
    {
        private readonly INavigationService _navigationService;

        public OnNavigationEntityDoAddAgent(Contexts contexts, INavigationService navigationService) : base(contexts.game)
        {
            _navigationService = navigationService;
        }

        protected override ICollector<GameEntity> GetTrigger(IContext<GameEntity> context)
        {
            return context.CreateCollector(GameMatcher.NavigationAware);
        }

        protected override bool Filter(GameEntity entity)
        {
            return entity.isNavigationAware;
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
