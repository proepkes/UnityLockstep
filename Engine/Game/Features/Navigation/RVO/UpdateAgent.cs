using System.Threading.Tasks;
using Entitas;
using FixMath.NET;

namespace Lockstep.Game.Features.Navigation.RVO
{
    class UpdateAgent : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _movableEntities;

        public UpdateAgent(Contexts contexts, ServiceContainer services)
        {
            _movableEntities = contexts.game.GetGroup(GameMatcher.AllOf(GameMatcher.LocalId, GameMatcher.Agent));
        }

        public void Execute()
        {
            Parallel.ForEach(_movableEntities.GetEntities(), entity =>
            {
                entity.ReplaceVelocity(entity.agent.velocity);
                entity.ReplacePosition(entity.position.value + entity.velocity.value);
            });
        }
    }
}
