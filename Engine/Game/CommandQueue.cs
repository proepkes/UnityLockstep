using System.Collections.Generic;
using System.Linq;
using Lockstep.Core.Logic;
using Lockstep.Core.Logic.Interfaces;

namespace Lockstep.Game
{

    public class CommandQueue : ICommandQueue
    {
        /// <summary>
        /// Mapping: FrameNumber -> Commands per player(Id)
        /// </summary>    
        public Dictionary<uint, List<Input>> Buffer { get; } = new Dictionary<uint, List<Input>>(5000);


        public void Enqueue(uint tick, byte actorId, params ICommand[] commands)
        {
            Enqueue(new Input(tick, actorId, commands));
        }

        public virtual void Enqueue(Input input)
        {
            lock (Buffer)
            {
                if (!Buffer.ContainsKey(input.Tick))
                {
                    Buffer.Add(input.Tick, new List<Input>(10)); //Initial size for 10 players
                }       

                Buffer[input.Tick].Add(input);
            }
        }

        public virtual List<Input> Dequeue()
        {
            lock (Buffer)
            {
                var result = Buffer.SelectMany(pair => pair.Value).ToList();
                Buffer.Clear();
                return result;
            }
        }
    }      
}
