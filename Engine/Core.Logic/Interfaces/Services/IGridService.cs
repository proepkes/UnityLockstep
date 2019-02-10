using BEPUutilities;

namespace Lockstep.Core.Logic.Interfaces.Services
{
    public interface IGridService : IService
    {
        Vector2 GetWorldSize();
        Vector2 GetCellSize();        
    }
}