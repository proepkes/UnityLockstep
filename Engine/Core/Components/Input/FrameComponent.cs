using Entitas;
using Entitas.CodeGeneration.Attributes;
using Lockstep.Core.Data;

namespace Lockstep.Core.Components.Input
{
    [Input, Unique]
    public class FrameComponent : IComponent
    {
        public Frame value;
    }
}