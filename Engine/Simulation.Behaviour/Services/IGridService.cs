using BEPUutilities;
using Lockstep.Core.Services;

namespace Simulation.Behaviour.Services
{
    public interface IGridService : IService
    {
        Vector2 GetWorldSize();
        Vector2 GetCellSize();        
    }
}