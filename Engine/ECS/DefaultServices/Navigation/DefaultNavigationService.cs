using System.Collections.Generic;
using System.Linq;
using BEPUutilities;
using FixMath.NET;

namespace ECS.DefaultServices.Navigation
{                                   
    public class DefaultNavigationService: INavigationService
    {
        private static readonly Fix64 AgentSpeed = Fix64.One;

        private readonly IList<Agent> _agents = new List<Agent>();

        public void AddAgent(int id, Vector2 position)
        {                
            _agents.Add(new Agent(id, position));
        }

        public void SetAgentDestination(int agentId, Vector2 newDestination)
        {
            _agents.First(agent => agent.Id == agentId).Destination = newDestination;  
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
