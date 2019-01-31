using Entitas;
using Lockstep.Core.Data;

namespace Lockstep.Core.Components.Game
{
    [Game]
    public class OwnerIdComponent : IComponent
    {                        
        public PlayerId value;
    }
}