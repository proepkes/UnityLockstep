using BEPUutilities;

namespace Lockstep.Core.Interfaces
{
    public interface IGridService : IService
    {
        Vector2 GetWorldSize();
        Vector2 GetCellSize();        
    }
}