using LiteNetLib.Utils;

namespace Lockstep.Framework.Networking.Messages
{
    public class Frame
    {                                               
        public uint FrameNumber { get; set; }
        public Command[] Commands { get; set; }


        public void Serialize(NetDataWriter writer)
        {
            writer.Put(FrameNumber);
            writer.Put(Commands.Length);
            foreach (var command in Commands)
            {
                writer.PutBytesWithLength(command.Data);
            }
        }


        public void Deserialize(NetDataReader reader)
        {
            FrameNumber = reader.GetUInt();
            var packetsLen = reader.GetInt();

            Commands = new Command[packetsLen];

            for (int i = 0; i < packetsLen; i++)
            {
                Commands[i] = new Command { Data = reader.GetBytesWithLength() };
            }
        }
    }     
}