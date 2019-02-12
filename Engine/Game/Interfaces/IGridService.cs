using BEPUutilities;
using Lockstep.Core.Logic.Interfaces;

namespace Lockstep.Game.Interfaces
{
    public interface IGridService : IService
    {
        Vector2 GetWorldSize();
        Vector2 GetCellSize();        
    }
}