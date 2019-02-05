using BEPUutilities;
using Lockstep.Core.Services;

namespace Lockstep.Game.Services
{
    public interface IGridService : IService
    {
        Vector2 GetWorldSize();
        Vector2 GetCellSize();        
    }
}