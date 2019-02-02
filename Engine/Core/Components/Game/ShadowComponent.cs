using Entitas;               

namespace Lockstep.Core.Components.Game
{
    [Game]
    //A shadow refers to an entity in the past
    public class ShadowComponent : IComponent
    {
        public uint localEntityIdRef;
    }
}
