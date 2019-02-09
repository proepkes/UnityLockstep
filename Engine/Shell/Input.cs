using System.Collections.Generic;
using Lockstep.Game.Interfaces;

namespace Lockstep.Game
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

        public override string ToString()
        {
            return ActorId + " >> " + Tick + ": " + Commands.GetType().FullName;
        }
    }
}