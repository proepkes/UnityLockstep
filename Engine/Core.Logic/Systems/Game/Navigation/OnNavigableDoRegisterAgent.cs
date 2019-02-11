using System.Collections.Generic;
using Entitas;
using Lockstep.Core.Logic.Interfaces.Services;

namespace Lockstep.Core.Logic.Systems.Game.Navigation
{
    public class OnNavigableDoRegisterAgent : ReactiveSystem<GameEntity>
    {
        private readonly INavigationService _navigationService;

        public OnNavigableDoRegisterAgent(Contexts contexts, ServiceContainer services) : base(contexts.game)
        {
            _navigationService = services.Get<INavigationService>();
        }

        protected override ICollector<GameEntity> GetTrigger(IContext<GameEntity> context)
        {
            return context.CreateCollector(GameMatcher.AllOf(GameMatcher.New, GameMatcher.Navigable));
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
