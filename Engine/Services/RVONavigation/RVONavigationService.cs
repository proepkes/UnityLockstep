using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BEPUutilities;                                
using Lockstep.Services.RVONavigation.RVO;

namespace Lockstep.Services.RVONavigation
{
    public class RVONavigationService 
    {                                                                                        
        public RVONavigationService()
        {
            Simulator.Instance.setTimeStep(0.5m);
            Simulator.Instance.setAgentDefaults(15, 10, 5, 5, 1, 2, new Vector2(0, 0));
        }
        
        public void AddAgent(uint id, Vector2 position)
        {
            Simulator.Instance.addAgent(id, new Vector2(position.X, position.Y));
        }

        public void RemoveAgent(uint id)
        {
            Simulator.Instance.removeAgent(id);    
        }

        public void SetAgentDestination(uint agentId, Vector2 newDestination)
        {
            Simulator.Instance.agents_[agentId].Destination = newDestination;
        }

        public void SetAgentPositions(Dictionary<uint, Vector2> positions)
        {
            foreach (var agentId in positions.Keys)
            {
                Simulator.Instance.agents_[agentId].Position = positions[agentId];
            }
        }

        public void Tick()
        {
            Parallel.ForEach(Simulator.Instance.agents_.Values, agent =>
            {
                agent.CalculatePrefVelocity();
            });

            Simulator.Instance.doStep();
        }

        public Dictionary<uint, Vector2> GetAgentVelocities()
        {
            return Simulator.Instance.agents_.ToDictionary(pair => pair.Key, pair => pair.Value.Velocity);
        }
    }
}
