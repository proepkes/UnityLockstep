using ECS.Data;
using LiteNetLib.Utils;
using Lockstep.Framework.Commands;

namespace Lockstep.Framework.Services
{
    public enum CommandTag : byte
    {
        Navigate,
    }

    public class DefaultInputParseService : IInputParseService
    {
        private readonly NetDataReader _dataReader;

        public DefaultInputParseService()
        {
            _dataReader = new NetDataReader();
        }

        public void Parse(InputContext context, SerializedInput serializedInput)
        {
            _dataReader.SetSource(serializedInput.Data);

            var commandTag = (CommandTag)_dataReader.GetByte();
            switch (commandTag)
            {
                case CommandTag.Navigate:
                    var cmd = new NavigateCommand();
                    cmd.Deserialize(_dataReader);

                    var e = context.CreateEntity();

                    e.isNavigationInput = true;
                    e.AddGameEntityIds(cmd.EntityIds);
                    e.AddMousePosition(cmd.Destination);
                         
                    break;
            }
        }        
    }
}