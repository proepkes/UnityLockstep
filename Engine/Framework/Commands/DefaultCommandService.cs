using System.Collections.Generic;
using ECS.Data;
using LiteNetLib.Utils;
using Lockstep.Framework.Commands;

namespace Lockstep.Framework.Services
{
    public enum CommandTag : ushort
    {
        Spawn,
        Navigate,
    }


    public class DefaultParseInputService : IParseInputService
    {
        private readonly NetDataReader _dataReader;
        private readonly Dictionary<CommandTag, ISerilalizableCommand> _commandMap = new Dictionary<CommandTag, ISerilalizableCommand>() ;

        public DefaultParseInputService()
        {
            _dataReader = new NetDataReader();
            _commandMap.Add(CommandTag.Spawn, new SpawnCommand());
            _commandMap.Add(CommandTag.Navigate, new NavigateCommand());
        }

        public void Parse(InputContext context, SerializedInput serializedInput)
        {
            _dataReader.SetSource(serializedInput.Data);

            var commandTag = (CommandTag)_dataReader.GetByte();

            var cmd = _commandMap[commandTag];
            cmd.Deserialize(_dataReader);
            cmd.Execute(context);
        }        
    }
}