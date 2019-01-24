using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BEPUutilities;   

namespace ECS.DefaultServices.Navigation
{                                   
    public class DefaultNavigationService: INavigationService
    {                                  
        private readonly Dictionary<int, Agent> _agents = new Dictionary<int, Agent>();      

        public void AddAgent(int id, Vector2 position)
        {
            _agents.Add(id, new Agent(position));
        }

        public void SetAgentDestination(int agentId, Vector2 newDestination)
        {
            _agents[agentId].Destination = newDestination;
        }

        public void Tick()
        {
            Parallel.ForEach(_agents.Values, agent => agent.Update());  
        }

        public Dictionary<int, Vector2> GetAgentPositions()
        {
            return _agents.ToDictionary(pair => pair.Key, pair => pair.Value.Position);
        }
    }
}
