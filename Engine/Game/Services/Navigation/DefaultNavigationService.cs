using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BEPUutilities;
using Lockstep.Core.Logic.Interfaces.Services;

namespace Lockstep.Game.Services.Navigation
{                                   
    public class DefaultNavigationService: INavigationService
    {                                  
        private readonly Dictionary<uint, Agent> _agents = new Dictionary<uint, Agent>();      

        public void AddAgent(uint id, Vector2 position)
        {       
            _agents.Add(id, new Agent(position));
        }

        public void RemoveAgent(uint id)
        {
            _agents.Remove(id);
        }

        public void SetAgentDestination(uint agentId, Vector2 newDestination)
        {
            _agents[agentId].Destination = newDestination;
        }

        public void SetAgentPositions(Dictionary<uint, Vector2> positions)
        {
            foreach (var pair in positions)
            {
                _agents[pair.Key].Position = pair.Value;
            }
        }

        public void Tick()
        {
            Parallel.ForEach(_agents.Values, agent => agent.Update());  
        }

        public Dictionary<uint, Vector2> GetAgentVelocities()
        {
            return _agents.ToDictionary(pair => pair.Key, pair => pair.Value.Velocity);
        }
    }
}
