using System;
using System.Collections.Generic;
using System.Linq;
using Lockstep.Core.Interfaces;

namespace Lockstep.Core
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

        public List<KeyValuePair<uint, Dictionary<uint, Dictionary<byte, List<ICommand>>>>> GetAllCommandsForFrame(uint frame)
        {                       
            return Log.Reverse().Where(pair => pair.Key == frame).ToList();
        }
    }
}
