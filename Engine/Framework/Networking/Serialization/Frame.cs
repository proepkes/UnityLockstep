using ECS.Data;
using LiteNetLib.Utils;

namespace Lockstep.Framework.Networking.Serialization
{
    public static class FrameExt
    {             
        public static void Serialize(this Frame frame, NetDataWriter writer, uint FrameNumber)
        {
            writer.Put(FrameNumber);
            writer.Put(frame.Commands.Length);
            foreach (var command in frame.Commands)
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

            frame.Commands = new Command[packetsLen];

            for (int i = 0; i < packetsLen; i++)
            {
                frame.Commands[i] = new Command { Data = reader.GetBytesWithLength() };
            }
        }
    }     
}