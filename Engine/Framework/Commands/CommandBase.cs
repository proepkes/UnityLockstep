using LiteNetLib.Utils;
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

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((ushort) Tag);
            OnSerialize(writer);
        }


        public void Deserialize(NetDataReader reader)
        {   
            Tag = (CommandTag) reader.GetUShort();
            OnDeserialize(reader);
        }

        protected abstract void OnSerialize(NetDataWriter writer);

        protected abstract void OnDeserialize(NetDataReader reader);

        public abstract void Execute(GameContext context);  
    }
}