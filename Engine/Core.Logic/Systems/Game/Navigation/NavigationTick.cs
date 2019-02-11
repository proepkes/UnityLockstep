using Entitas;
using FixMath.NET;
using Lockstep.Core.Logic.Interfaces.Services;

namespace Lockstep.Core.Logic.Systems.Game.Navigation
{
    public class NavigationTick : IExecuteSystem
    {
        private readonly Contexts _contexts;
        private readonly INavigationService _navigationService;

        public NavigationTick(Contexts contexts, ServiceContainer services)
        {
            _contexts = contexts;
            _navigationService = services.Get<INavigationService>();
        }

        public void Execute()
        {
            //_navigationService.Tick();  

            //All registered (navigable) entities have to be updated, because avoidance could move other entities aside
            foreach (var entity in _contexts.game.GetEntities(GameMatcher.AllOf(GameMatcher.LocalId, GameMatcher.Destination)))
            {
                var velocity = entity.destination.value - entity.position.value;
                if (velocity.LengthSquared() > Fix64.One)
                {
                    velocity.Normalize();
                }

                if ((entity.destination.value - entity.position.value).LengthSquared() > 1)
                {
                    entity.ReplacePosition(entity.position.value + velocity);        
                }
            }
        }
    }
}
