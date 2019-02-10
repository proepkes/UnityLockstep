using Lockstep.Core.Logic.Serialization.Utils;

namespace Lockstep.Core.Logic.Serialization
{
    public interface ISerializable
    {
        void Serialize(Serializer writer);

        void Deserialize(Deserializer reader);
    }
}