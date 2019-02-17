using Entitas;
using FixMath.NET;

namespace Lockstep.Game.Features.Navigation.RVO
{
    class ComputeAgentPreferredVelocity : IExecuteSystem
    {
        private readonly Contexts _contexts;
        private readonly IGroup<GameEntity> _agents;

        public ComputeAgentPreferredVelocity(Contexts contexts, ServiceContainer services)
        {
            _contexts = contexts;

            _agents = _contexts.game.GetGroup(GameMatcher.AllOf(GameMatcher.LocalId, GameMatcher.Agent));
        }

        public void Execute()
        {
            foreach (var agent in _agents)
            {
                var velocity = agent.destination.value - agent.position.value;
                if (velocity.LengthSquared() > Fix64.One)
                {
                    velocity.Normalize();
                }

                agent.agent.preferredVelocity = velocity;
            }
        }
    }
}
