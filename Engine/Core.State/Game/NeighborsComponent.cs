using Entitas;

namespace Lockstep.Core.State.Game
{
    /// <summary>
    /// Contains an array of the nearest neighbors (referenced by LocalId).
    /// Not pure ECS style (a neighbors-component does not really say anything about the entity itself), so this will probably go into a service in the future
    /// </summary>
    public class NeighborsComponent : IComponent
    {
        public uint[] neighborsDefault;
        public uint[] neighborsECS;
    }
}
