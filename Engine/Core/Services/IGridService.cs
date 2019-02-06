using BEPUutilities;

namespace Lockstep.Core.Services
{
    public interface IGridService : IService
    {
        Vector2 GetWorldSize();
        Vector2 GetCellSize();        
    }
}