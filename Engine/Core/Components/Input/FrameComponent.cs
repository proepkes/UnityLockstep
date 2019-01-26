using Entitas;
using Entitas.CodeGeneration.Attributes;
using Lockstep.Core.Data;

namespace Lockstep.Core.Components.Input
{
    [Input, Unique]
    public class CommandsComponent : IComponent
    {
        public ICommand[] input;
    }
}