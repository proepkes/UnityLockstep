using Lockstep.Network.Utils;

namespace Lockstep.Network
{
    public interface ISerializable
    {
        void Serialize(Serializer writer);

        void Deserialize(Deserializer reader);
    }
}