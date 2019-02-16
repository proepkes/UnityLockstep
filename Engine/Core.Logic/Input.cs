using System.Collections.Generic;
using Lockstep.Core.Logic.Interfaces;

namespace Lockstep.Core.Logic
{
    public class Input
    {
        public uint Tick { get; }
        public byte ActorId { get; }
        public IEnumerable<ICommand> Commands { get; }

        public Input(uint tick, byte actorId, IEnumerable<ICommand> commands)
        {
            Tick = tick;
            ActorId = actorId;
            Commands = commands;
        }
    }
}