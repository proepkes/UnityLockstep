using ECS.Data;
using LiteNetLib.Utils;
using Lockstep.Framework.Commands;

namespace Lockstep.Framework.Services
{
    public enum CommandTag : byte
    {
        Navigate,
    }


    public class DefaultCommandService : ICommandService
    {
        private readonly NetDataReader _dataReader;

        public DefaultCommandService()
        {
            _dataReader = new NetDataReader();
        }

        public void Process(InputContext context, Command command)
        {
            _dataReader.SetSource(command.Data);

            var commandTag = (CommandTag)_dataReader.GetByte();
            switch (commandTag)
            {
                case CommandTag.Navigate:
                    var cmd = new NavigateCommand();
                    cmd.Deserialize(_dataReader);

                    var e = context.CreateEntity();
                    e.AddGameEntityIds(cmd.EntityIds);
                    e.AddInputPosition(cmd.Destination);
                         
                    break;
            }
        }        
    }
}