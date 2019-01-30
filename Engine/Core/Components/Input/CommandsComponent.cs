using System.Collections.Generic;
using Entitas;
using Entitas.CodeGeneration.Attributes;
using Lockstep.Core.Interfaces;

namespace Lockstep.Core.Components.Input
{
    [Input, Unique]
    public class CommandsComponent : IComponent
    {
        public Dictionary<byte, List<ICommand>> input;
    }
}