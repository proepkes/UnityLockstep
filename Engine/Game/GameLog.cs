using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Lockstep.Core.Logic;
using Lockstep.Core.Logic.Interfaces;

namespace Lockstep.Game
{
    /// <summary>
    /// Stores all inputs including the tick in which the input was added. Can be used to exactly re-simulate a game (including rollback/prediction)
    /// </summary>
    [Serializable]
    public class GameLog
    {
        public byte LocalActorId { get; set; }
        public byte[] AllActorIds { get; set; }

        public Dictionary<uint, Dictionary<uint, Dictionary<byte, List<ICommand>>>> InputLog { get; } = new Dictionary<uint, Dictionary<uint, Dictionary<byte, List<ICommand>>>>();

        public void Add(uint tick, uint targetTick, byte actorId, params ICommand[] commands)
        {                                          
            Add(tick, new Input(targetTick, actorId, commands));
        }

        public void Add(uint tick, Input input)
        {
            if (!InputLog.ContainsKey(tick))
            {
                InputLog.Add(tick, new Dictionary<uint, Dictionary<byte, List<ICommand>>>());
            }

            if (!InputLog[tick].ContainsKey(input.Tick))
            {
                InputLog[tick].Add(input.Tick, new Dictionary<byte, List<ICommand>>());
            }

            if (!InputLog[tick][input.Tick].ContainsKey(input.ActorId))
            {
                InputLog[tick][input.Tick].Add(input.ActorId, new List<ICommand>());
            }

            InputLog[tick][input.Tick][input.ActorId].AddRange(input.Commands);
        }

        public void WriteTo(Stream stream)
        {
            var formatter = new BinaryFormatter();
            formatter.Serialize(stream, this);
        }

        public static GameLog ReadFrom(Stream stream)
        {
            IFormatter formatter = new BinaryFormatter();
            return (GameLog)formatter.Deserialize(stream);
        }
    }
}
