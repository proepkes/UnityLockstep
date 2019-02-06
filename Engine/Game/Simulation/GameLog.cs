using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Lockstep.Core.Commands;

namespace Lockstep.Game.Simulation
{
    [Serializable]
    public class GameLog
    {
        public Dictionary<uint, Dictionary<uint, Dictionary<byte, List<ICommand>>>> Log { get; } = new Dictionary<uint, Dictionary<uint, Dictionary<byte, List<ICommand>>>>();
        public void Add(uint tickId, uint targetTickId, byte actorId, IEnumerable<ICommand> commands)
        {
            if (!Log.ContainsKey(tickId))
            {                                                                          
                Log.Add(tickId, new Dictionary<uint, Dictionary<byte, List<ICommand>>>());
            }

            if (!Log[tickId].ContainsKey(targetTickId))
            {                                                                      
                Log[tickId].Add(targetTickId, new Dictionary<byte, List<ICommand>>());
            }

            if (!Log[tickId][targetTickId].ContainsKey(actorId))
            {
                Log[tickId][targetTickId].Add(actorId, new List<ICommand>());
            }

            Log[tickId][targetTickId][actorId].AddRange(commands);
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
