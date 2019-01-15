
using ECS.Data;
using LiteNetLib.Utils;

class DefaultCommandService : ICommandService
{
    private NetDataReader dataReader;

    public DefaultCommandService()
    {
        dataReader = new NetDataReader();
    }

    public void Process(InputContext context, Command command)
    {
        dataReader.SetSource(command.Data);
        
    }        
}  