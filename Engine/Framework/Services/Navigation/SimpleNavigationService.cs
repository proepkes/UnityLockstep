using System.Collections.Generic;
using System.Linq;
using BEPUutilities;
using FixMath.NET;
using Lockstep.Framework.Services.Navigation.Simple;

namespace Lockstep.Framework.Services.Navigation
{                                   
    public class SimpleNavigationService: INavigationService
    {
        private static readonly Fix64 AgentSpeed = Fix64.One;

        private readonly IList<Agent> _agents = new List<Agent>();

        public void AddAgent(GameEntity entity, Vector2 position)
        {                
            _agents.Add(new Agent(entity.id.value, position));
        }

        public void UpdateDestination(int[] agentIds, Vector2 newDestination)
        {
            foreach (var agent in _agents.Where(agent => agentIds.Contains(agent.Id)))
            {
                agent.Destination = newDestination;
            }    
        }

        public void UpdateAgents()
        {
            foreach (var agent in _agents)
            {
                var goal = Vector2.Normalize(agent.Destination - agent.Position);

                agent.Position += goal * AgentSpeed;
            }
        }

        public Vector2 GetAgentPosition(int agentId)
        {
            return _agents.First(agent => agent.Id == agentId).Position;
        }     
    }
}
