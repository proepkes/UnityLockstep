using Entitas;

namespace Lockstep.Core.Components.Actor
{
    [Actor]
    //A shadow refers to an entity in the past
    public class ShadowComponent : IComponent
    {
        public byte actorIdRef;
    }
}
