using System;
using System.Collections.Generic;
using System.Linq;
using Lockstep.Core.Interfaces;

namespace Lockstep.Client.Implementations
{              
    [Serializable]
    public class CommandBuffer : ICommandBuffer
    {                                   
        /// <summary>
        /// Mapping: FrameNumber -> Commands per player(Id)
        /// </summary>    
        public Dictionary<uint, Dictionary<byte, List<ICommand>>> Buffer { get; } = new Dictionary<uint, Dictionary<byte, List<ICommand>>>(5000);
                                                                  

        public virtual void Insert(uint frameNumber, byte commanderId, ICommand[] commands)
        {
            lock (Buffer)
            {        
                if (!Buffer.ContainsKey(frameNumber))
                {
                    Buffer.Add(frameNumber, new Dictionary<byte, List<ICommand>>(10)); //Initial size for 10 players
                }

                if (!Buffer[frameNumber].ContainsKey(commanderId))
                {
                    Buffer[frameNumber].Add(commanderId, new List<ICommand>(5)); //Initial size of 5 commands per frame per player
                }

                Buffer[frameNumber][commanderId].AddRange(commands);      
            }     
        }

        public Dictionary<uint, Dictionary<byte, List<ICommand>>> GetChanges()
        {
            lock (Buffer)
            {   
                var result = Buffer.ToDictionary(pair => pair.Key, pair => pair.Value);
                Buffer.Clear();
                return result;
            }
        }

        //public Dictionary<byte, List<ICommand>> Get(uint frame)
        //{
        //    lock (Buffer)
        //    {
        //        //If no commands were inserted then return an empty list
        //        if (!Buffer.ContainsKey(frame))
        //        {
        //            Buffer.Add(frame, new Dictionary<byte, List<ICommand>>());
        //        }

        //        return Buffer[frame];
        //    }
        //}       
    }
}
