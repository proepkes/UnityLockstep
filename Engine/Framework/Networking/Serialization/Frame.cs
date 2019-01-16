using ECS.Data;
using LiteNetLib.Utils;

namespace Lockstep.Framework.Networking.Serialization
{
    public static class FrameExt
    {             
        public static void Serialize(this Frame frame, NetDataWriter writer, uint FrameNumber)
        {
            writer.Put(FrameNumber);
            writer.Put(frame.SerializedInputs.Length);
            foreach (var command in frame.SerializedInputs)
            {
                writer.PutBytesWithLength(command.Data);
            }
        }    

        public static void Deserialize(this Frame frame, NetDataReader reader)
        {
            frame.Deserialize(reader, out _);
        }

        public static void Deserialize(this Frame frame, NetDataReader reader, out uint frameNumber)
        {
            frameNumber = reader.GetUInt();
            var packetsLen = reader.GetInt();

            frame.SerializedInputs = new SerializedInput[packetsLen];

            for (int i = 0; i < packetsLen; i++)
            {
                frame.SerializedInputs[i] = new SerializedInput { Data = reader.GetBytesWithLength() };
            }
        }
    }     
}