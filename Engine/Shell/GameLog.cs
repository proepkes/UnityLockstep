using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;             
using Lockstep.Game.Interfaces;        

namespace Lockstep.Game
{
    /// <summary>
    /// Stores all inputs including the tick in which the input was added. Can be used to exactly re-simulate a game (including rollback/prediction)
    /// </summary>
    [Serializable]
    public class GameLog
    {
        public Dictionary<uint, Dictionary<uint, Dictionary<byte, List<ICommand>>>> Log { get; } = new Dictionary<uint, Dictionary<uint, Dictionary<byte, List<ICommand>>>>();
        public void Add(uint tickId, Input input)
        {
            if (!Log.ContainsKey(tickId))
            {
                Log.Add(tickId, new Dictionary<uint, Dictionary<byte, List<ICommand>>>());
            }

            if (!Log[tickId].ContainsKey(input.Tick))
            {
                Log[tickId].Add(input.Tick, new Dictionary<byte, List<ICommand>>());
            }

            if (!Log[tickId][input.Tick].ContainsKey(input.ActorId))
            {
                Log[tickId][input.Tick].Add(input.ActorId, new List<ICommand>());
            }

            Log[tickId][input.Tick][input.ActorId].AddRange(input.Commands);
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
