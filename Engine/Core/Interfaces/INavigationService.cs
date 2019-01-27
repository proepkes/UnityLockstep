using System.Collections.Generic;
using BEPUutilities;

namespace Lockstep.Core.Interfaces
{
    public interface INavigationService : IService
    {
        void AddAgent(int id, Vector2 position);

        void SetAgentDestination(int agentId, Vector2 newDestination);

        void SetAgentPositions(Dictionary<int, Vector2> positions);

        void Tick();

        Dictionary<int, Vector2> GetAgentVelocities();
    }
}