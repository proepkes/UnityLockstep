using System.Collections.Generic;
using ECS;
using ECS.Data;
using Lockstep.Framework.Commands;

namespace Lockstep.Framework.Services
{
    public enum CommandTag : ushort
    {
        Spawn,
        Navigate,
    }


    public class ParseInputService : IParseInputService
    {
        private readonly INetworkReader _networkReader;   
        private readonly Dictionary<CommandTag, ISerilalizableCommand> _commandMap = new Dictionary<CommandTag, ISerilalizableCommand>() ;

        public ParseInputService(INetworkReader networkReader)
        {
            _networkReader = networkReader;    
            _commandMap.Add(CommandTag.Spawn, new SpawnCommand());
            _commandMap.Add(CommandTag.Navigate, new NavigateCommand());
        }

        public void Parse(InputContext context, SerializedInput serializedInput)
        {
            _networkReader.SetSource(serializedInput.Data);
                                                             
            var commandTag = (CommandTag)_networkReader.PeekUShort();

            var cmd = _commandMap[commandTag];
            cmd.Deserialize(_networkReader);
            cmd.Execute(context);
        }        
    }
}