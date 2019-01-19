using System.Collections.Generic;
using System.Linq;    
using BEPUutilities;
using FixMath.NET;
using Lockstep.Framework.Services.Pathfinding.Simple;

namespace Lockstep.Framework.Services.Pathfinding
{                                   
    public class SimplePathfinderService: IPathfindingService
    {
        private static readonly Fix64 AgentSpeed = Fix64.One;

        readonly IList<Agent> _agents = new List<Agent>();

        public void AddAgent(GameEntity entity, Vector2 position)
        {                
            _agents.Add(new Agent(entity.id.value, position));
        }

        public void UpdateAgents(GameEntity[] entities)
        {
            foreach (var entity in entities)
            {
                var goal = Vector2.Normalize(entity.destination.value - entity.position.value);

                _agents.First(a => a.Id == entity.id.value).Position += goal * AgentSpeed;
            }
        }

        public Vector2 GetAgentPosition(int agentId)
        {
            return _agents.First(agent => agent.Id == agentId).Position;
        }     
    }
}
