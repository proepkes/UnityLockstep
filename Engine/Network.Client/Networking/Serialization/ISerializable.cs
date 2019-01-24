namespace Lockstep.Framework.Networking.Serialization
{
    public interface ISerializable
    {
        void Serialize(ISerializer serializer);

        void Deserialize(IDeserializer deserializer);
    }
}