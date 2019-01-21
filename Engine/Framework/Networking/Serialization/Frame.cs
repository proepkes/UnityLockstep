using ECS.Data;

namespace Lockstep.Framework.Networking.Serialization
{
    public static class FrameExt
    {             
        public static void Serialize(this Frame frame, INetworkWriter writer, uint FrameNumber)
        {
            writer.Put(FrameNumber);
            writer.Put(frame.SerializedInputs.Length);
            foreach (var command in frame.SerializedInputs)
            {
                writer.PutBytesWithLength(command.Data);
            }
        }    

        public static void Deserialize(this Frame frame, INetworkReader reader)
        {
            frame.Deserialize(reader, out _);
        }

        public static void Deserialize(this Frame frame, INetworkReader reader, out uint frameNumber)
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