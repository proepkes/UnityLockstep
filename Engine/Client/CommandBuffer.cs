using System.Collections.Generic;
using System.Linq;
using Lockstep.Core.Data;
using Lockstep.Core.Interfaces;

namespace Lockstep.Client.Implementations
{                
    public class CommandBuffer : ICommandBuffer
    {                                   
        private readonly Dictionary<ulong, List<ICommand>> _commands = new Dictionary<ulong, List<ICommand>>();

        public ulong Count
        {
            get
            {
                lock (_commands)
                {
                    return _commands.Keys.Max();
                }
            }
        }

        public ulong ItemIndex { get; private set; }

        public ulong Remaining => Count - ItemIndex;

        public void Insert(ulong frameNumber, ICommand command)
        {
            lock (_commands)
            {
                if (!_commands.ContainsKey(frameNumber))
                {
                    _commands.Add(frameNumber, new List<ICommand>(10));
                }

                _commands[frameNumber].Add(command);
            }              
        }

        public ICommand[] GetNext()
        {                    
            lock (_commands)
            {
                //If no commands were inserted then return an empty list
                if (!_commands.ContainsKey(ItemIndex))
                {
                    _commands[ItemIndex] = new List<ICommand>();
                }

                return _commands[ItemIndex++].ToArray();    

            }                 
        }
    }
}
