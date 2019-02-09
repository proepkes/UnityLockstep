using Entitas;
using FixMath.NET;
using Lockstep.Core.Services;

namespace Lockstep.Core.Systems.Navigation
{
    public class NavigationTick : IExecuteSystem
    {
        private readonly Contexts _contexts;
        private readonly INavigationService _navigationService;

        public NavigationTick(Contexts contexts, INavigationService navigationService)
        {
            _contexts = contexts;
            _navigationService = navigationService;
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
                    
                    //For Testing purposes:
                    //entity.ReplacePosition(entity.destination.value);
                }
            }
        }
    }
}
