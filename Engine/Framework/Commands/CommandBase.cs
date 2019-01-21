using Lockstep.Framework.Networking;
using Lockstep.Framework.Services;

namespace Lockstep.Framework.Commands
{
    public abstract class CommandBase : ISerilalizableCommand 
    {
        public CommandTag Tag { get; private set; }

        public CommandBase(CommandTag tag)
        {
            Tag = tag;
        }     

        public void Serialize(INetworkWriter writer)
        {
            writer.Put((ushort) Tag);
            OnSerialize(writer);
        }


        public void Deserialize(INetworkReader reader)
        {   
            Tag = (CommandTag) reader.GetUShort();
            OnDeserialize(reader);
        }

        protected abstract void OnSerialize(INetworkWriter writer);

        protected abstract void OnDeserialize(INetworkReader reader);

        public abstract void Execute(InputContext context);  
    }
}