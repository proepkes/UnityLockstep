using System;
using ECS.Data;
using Lockstep.Framework.Commands;

namespace Lockstep.Framework.Networking.Serialization
{                     
    public class InputParser
    {                                                  
        private readonly Func<IDeserializer, Command> _createCommandDelegate; 

        public InputParser(Func<IDeserializer, Command> createCommandDelegate)
        {                               
            _createCommandDelegate = createCommandDelegate;
        }  

        public Frame DeserializeInput(IDeserializer deserializer)
        {
            var resultFrame = new Frame();
                                                      
            var commandsLength = deserializer.GetInt();

            resultFrame.Commands = new ICommand[commandsLength];
            for (int i = 0; i < commandsLength; i++)
            {
                var command = _createCommandDelegate.Invoke(deserializer);
                command.Deserialize(deserializer);

                resultFrame.Commands[i] = command;
            }

            return resultFrame;
        }            
    }
}