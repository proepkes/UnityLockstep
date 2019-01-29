using System.Collections.Generic;
using System.Linq;              
using Lockstep.Client.Interfaces;
using Lockstep.Core.Interfaces;

namespace Lockstep.Client.Implementations
{                
    public class CommandBuffer : ICommandBuffer
    {                                   
        /// <summary>
        /// Mapping: FrameNumber -> Commands per player(Id)
        /// </summary>    
        protected Dictionary<uint, Dictionary<byte, List<ICommand>>> Buffer { get; } = new Dictionary<uint, Dictionary<byte, List<ICommand>>>(5000);

        public uint LastInsertedFrame { get; private set; }      

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
                    Buffer[frameNumber].Add(commanderId, new List<ICommand>(5)); //Initial size of 5 commands per frame
                }

                //TODO: order by timestamp in case of multiple commands in the same frame => if commands intersect, the first one should win, requires !serverside! timestamp
                //ordering is enough, validation should take place in the simulation(core)
                Buffer[frameNumber][commanderId].AddRange(commands);

                LastInsertedFrame = frameNumber;
            }     
        }

        public Dictionary<byte, List<ICommand>> Get(uint frame)
        {
            lock (Buffer)
            {
                //If no commands were inserted then return an empty list
                if (!Buffer.ContainsKey(frame))
                {
                    Buffer.Add(frame, new Dictionary<byte, List<ICommand>>());
                }

                return Buffer[frame];
            }
        }

        public ICommand[] GetMany(uint frame)
        {
            lock (Buffer)
            {
                //If no commands were inserted then return an empty list
                if (!Buffer.ContainsKey(frame))
                {
                    Buffer.Add(frame, new Dictionary<byte, List<ICommand>>());
                }

                return Buffer[frame].SelectMany(pair => pair.Value).ToArray();
            }
        }
               
    }
}
