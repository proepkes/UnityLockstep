using System.Collections.Generic;
using ECS.Data;
using LiteNetLib.Utils;
using Lockstep.Framework.Commands;

namespace Lockstep.Framework.Services
{
    public enum CommandTag : byte
    {
        Spawn,
        Navigate,
    }

    public class DefaultInputParseService : IInputParseService
    {
        private readonly NetDataReader _dataReader;
        private readonly Dictionary<CommandTag, ISerilalizableCommand> _commandMap = new Dictionary<CommandTag, ISerilalizableCommand>() ;

        public DefaultInputParseService()
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
            InputEntity e;
            switch (commandTag)
            {
                case CommandTag.Spawn:           
                    e = context.CreateEntity();
                    e.AddSpawnInput(((SpawnCommand)cmd).AssetName);                              
                    break;
                case CommandTag.Navigate:            

                    e = context.CreateEntity();
                    e.isNavigationInput = true;
                    e.AddGameEntityIds(((NavigateCommand)cmd).EntityIds);
                    e.AddMousePosition(((NavigateCommand)cmd).Destination);

                    break;
            }
        }        
    }
}