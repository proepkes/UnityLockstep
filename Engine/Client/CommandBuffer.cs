using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Lockstep.Core.Data;
using Lockstep.Core.Interfaces;

namespace Lockstep.Client.Implementations
{                
    public class CommandBuffer : ICommandBuffer
    {                                   
        /// <summary>
        /// Mapping: FrameNumber -> Commands per player(Id)
        /// </summary>    
        public Dictionary<uint, Dictionary<byte, List<ICommand>>> Buffer { get; } = new Dictionary<uint, Dictionary<byte, List<ICommand>>>(5000);

        public uint NextFrameIndex { get; set; }    

        public virtual void Insert(byte commanderId, uint frameNumber, ICommand[] commands)
        {
            lock (Buffer)
            {        
                if (!Buffer.ContainsKey(frameNumber))
                {
                    Buffer.Add(frameNumber, new Dictionary<byte, List<ICommand>>(10)); //Initial size for 10 players
                }

                if (!Buffer[frameNumber].ContainsKey(commanderId))
                {
                    Buffer[frameNumber].Add(commanderId, new List<ICommand>(5)); //Initial size of 5 commands per frame
                }

                //TODO: order by timestamp in case of multiple commands in the same frame => if commands intersect, the first one should win, requires !serverside! timestamp
                //ordering is enough, validation should take place in the simulation(core)
                Buffer[frameNumber][commanderId].AddRange(commands);

                if (frameNumber < NextFrameIndex)
                {
                    NextFrameIndex = frameNumber;
                }
            }     
        }

        public ICommand[] GetNext()
        {
            lock (Buffer)
            {      
                //If no commands were inserted then return an empty list
                if (!Buffer.ContainsKey(NextFrameIndex))
                {
                    Buffer.Add(NextFrameIndex, new Dictionary<byte, List<ICommand>>());
                }

                return Buffer[NextFrameIndex++].SelectMany(pair => pair.Value).ToArray();
            }
        }
    }
}
