using System.Linq;
using BEPUutilities;
using Entitas;
using Lockstep.Common;
using Lockstep.Game.Features.Navigation.RVO.Algorithm;

namespace Lockstep.Game.Features.Navigation.RVO
{
    public class NavigationTick : IInitializeSystem, IExecuteSystem
    {
        private readonly Contexts _contexts;

        public NavigationTick(Contexts contexts, ServiceContainer services)
        {
            _contexts = contexts;
        }
        
        public void Initialize()
        {
            Simulator.Instance.setTimeStep(0.5m);
            Simulator.Instance.setAgentDefaults(15, 10, 5, 5, 1, 1);
        }

        public void Execute()
        {
            var entities = _contexts.game.GetEntities(GameMatcher.AllOf(GameMatcher.LocalId, GameMatcher.RvoAgentSettings, GameMatcher.Destination));

            //TODO: highly inefficient, but works for a start... rewrite should make use of entity-components (also no separate agent-class).
            //ordering the entities is important! if there are more than <MAX_NEIGHBORS> neighbors, the tree must choose the same neighbors everytime. it could happen that the default ordering differs on the client due to rollback/prediction
            Simulator.Instance.agents_.Clear();
            foreach (var entity in entities.OrderBy(entity => entity.actorId.value).ThenBy(entity => entity.id.value))
            {
                Simulator.Instance.addAgent(entity.localId.value, entity.position.value, entity.destination.value);
            }
            foreach (var (_, agent) in Simulator.Instance.agents_)
            {
                agent.CalculatePrefVelocity();
            }
            Simulator.Instance.doStep();


            foreach (var (agentId, agent) in Simulator.Instance.agents_)
            {
                var entity = _contexts.game.GetEntityWithLocalId(agentId);
                var newPosition = entity.position.value + agent.Velocity;
                if ((newPosition - entity.position.value).LengthSquared() < F64.C0p5)
                {
                    entity.RemoveDestination();
                }

                entity.ReplacePosition(entity.position.value + agent.Velocity);
            }
        }
    }
}
