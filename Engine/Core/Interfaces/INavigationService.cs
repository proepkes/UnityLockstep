using System.Collections.Generic;
using BEPUutilities;

namespace Lockstep.Core.Interfaces
{
    public interface INavigationService : IService
    {
        void AddAgent(uint id, Vector2 position);

        void RemoveAgent(uint id);

        void SetAgentDestination(uint agentId, Vector2 newDestination);

        void SetAgentPositions(Dictionary<uint, Vector2> positions);

        void Tick();

        Dictionary<uint, Vector2> GetAgentVelocities();
    }
}