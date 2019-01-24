using System;
using ECS.Data;                       

namespace Lockstep.Framework.Networking.Serialization
{                     
    public class InputParser
    {                                                  
        private readonly Func<IDeserializer, ICommand> _createCommandDelegate; 

        public InputParser(Func<IDeserializer, ICommand> createCommandDelegate)
        {                               
            _createCommandDelegate = createCommandDelegate;
        }  

        public ICommand[] DeserializeInput(IDeserializer deserializer)
        {        
            var commandsLength = deserializer.GetInt();

            var result = new ICommand[commandsLength];
                                                                 
            for (var i = 0; i < commandsLength; i++)
            {                                         
                var command = _createCommandDelegate.Invoke(deserializer);
                if (command != null)
                {
                    result[i] = command;
                }
            }

            return result;
        }            
    }
}