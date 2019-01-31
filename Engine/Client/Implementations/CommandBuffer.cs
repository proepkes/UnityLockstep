using System.Collections.Generic;
using System.Linq;              
using Lockstep.Client.Interfaces;
using Lockstep.Core.Data;
using Lockstep.Core.Interfaces;

namespace Lockstep.Client.Implementations
{                
    public class CommandBuffer : ICommandBuffer
    {                                   
        /// <summary>
        /// Mapping: FrameNumber -> Commands per player(Id)
        /// </summary>    
        protected Dictionary<TickId, Dictionary<PlayerId, List<ICommand>>> Buffer { get; } = new Dictionary<TickId, Dictionary<PlayerId, List<ICommand>>>(5000);

        public uint LastInsertedFrame { get; private set; }      

        public virtual void Insert(TickId frameNumber, PlayerId commanderId, ICommand[] commands)
        {
            lock (Buffer)
            {        
                if (!Buffer.ContainsKey(frameNumber))
                {
                    Buffer.Add(frameNumber, new Dictionary<PlayerId, List<ICommand>>(10)); //Initial size for 10 players
                }

                if (!Buffer[frameNumber].ContainsKey(commanderId))
                {
                    Buffer[frameNumber].Add(commanderId, new List<ICommand>(5)); //Initial size of 5 commands per frame per player
                }

                Buffer[frameNumber][commanderId].AddRange(commands);

                LastInsertedFrame = frameNumber;
            }     
        }

        public Dictionary<PlayerId, List<ICommand>> Get(TickId frame)
        {
            lock (Buffer)
            {
                //If no commands were inserted then return an empty list
                if (!Buffer.ContainsKey(frame))
                {
                    Buffer.Add(frame, new Dictionary<PlayerId, List<ICommand>>());
                }

                return Buffer[frame];
            }
        }       
    }
}
