using Entitas;
using FixMath.NET;

namespace Lockstep.Game.Features.Navigation.RVO
{
    class UpdatePreferredVelocity : IExecuteSystem
    {
        private readonly Contexts _contexts;
        private readonly IGroup<GameEntity> movingEntities;

        public UpdatePreferredVelocity(Contexts contexts, ServiceContainer services)
        {
            _contexts = contexts;

            movingEntities = _contexts.game.GetGroup(GameMatcher.AllOf(GameMatcher.LocalId, GameMatcher.RvoAgentSettings));
        }

        public void Execute()
        {
            foreach (var entity in movingEntities)
            {
                var velocity = entity.destination.value - entity.position.value;
                if (velocity.LengthSquared() > Fix64.One)
                {
                    velocity.Normalize();
                }

                entity.rvoAgentSettings.preferredVelocity = velocity;
            }
        }
    }
}
