using Lockstep.Framework.Networking;

namespace Lockstep.Framework.Commands
{
    public interface ISerilalizableCommand
    {
        void Serialize(INetworkWriter writer);

        void Deserialize(INetworkReader reader);

        void Execute(InputContext context);
    }
}