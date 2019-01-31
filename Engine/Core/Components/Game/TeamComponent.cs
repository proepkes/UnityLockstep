using Entitas;
using Lockstep.Core.Data;

namespace Lockstep.Core.Components.Game
{
    [Game]
    public class TeamComponent : IComponent
    {
        public TeamId value;
    }
}