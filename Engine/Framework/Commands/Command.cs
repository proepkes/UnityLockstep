using Lockstep.Framework.Networking;
using Lockstep.Framework.Networking.Serialization;

namespace Lockstep.Framework.Commands
{
    public abstract class Command : ICommand
    {
        public CommandTag Tag;  

        protected Command(CommandTag tag)
        {
            Tag = tag;            
        }       

        public void Serialize(ISerializer writer)
        {    
            writer.Put((ushort) Tag); 
            OnSerialize(writer);
        }            

        public void Deserialize(IDeserializer reader)
        {
            Tag = (CommandTag) reader.GetUShort();  
            OnDeserialize(reader);
        }

        protected abstract void OnSerialize(ISerializer writer);

        protected abstract void OnDeserialize(IDeserializer reader);

        public abstract void Execute(InputContext context);
    }
}