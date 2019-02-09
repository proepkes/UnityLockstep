using System.Collections.Generic;
using Entitas;
using Simulation.Behaviour.Services;

namespace Simulation.Behaviour.Game.Navigation
{
    public class OnNavigableDoRegisterAgent : ReactiveSystem<GameEntity>
    {
        private readonly INavigationService _navigationService;

        public OnNavigableDoRegisterAgent(Contexts contexts, INavigationService navigationService) : base(contexts.game)
        {
            _navigationService = navigationService;
        }

        protected override ICollector<GameEntity> GetTrigger(IContext<GameEntity> context)
        {
            return context.CreateCollector(GameMatcher.Navigable.Added());
        }

        protected override bool Filter(GameEntity entity)
        {
            return entity.hasLocalId && entity.hasPosition && entity.isNavigable;
        }

        protected override void Execute(List<GameEntity> entities)
        {
            foreach (var entity in entities)
            {
                _navigationService.AddAgent(entity.localId.value, entity.position.value); 
            } 
        }
    }
}
