using Entitas;
using FixMath.NET;

namespace Lockstep.Game.Features.Navigation
{
    public class NavigationTick : IExecuteSystem
    {
        private readonly Contexts _contexts;                      

        public NavigationTick(Contexts contexts, ServiceContainer services)
        {
            _contexts = contexts;                                        
        }

        public void Execute()
        {                                 
            //All registered (navigable) entities have to be updated (even those without a destination), because avoidance could move them aside
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
