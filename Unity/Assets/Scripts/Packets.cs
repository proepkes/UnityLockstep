       
using FixMath.NET;
using LiteNetLib.Utils;

public interface ICommandPacket
{                                        
    void Serialize(NetDataWriter writer);

    void Deserialize(NetDataReader reader);
}

public class MovePacket : ICommandPacket 
{        
    public ushort[] AgentIDs { get; set; }
    public Fix64 PosX { get; set; }
    public Fix64 PosY { get; set; }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put((long)PosX);
        writer.Put((long)PosY);
        writer.PutArray(AgentIDs);
    }

    public void Deserialize(NetDataReader reader)
    {
        PosX = reader.GetLong();
        PosY = reader.GetLong();
        AgentIDs = reader.GetUShortArray();
    }
}
