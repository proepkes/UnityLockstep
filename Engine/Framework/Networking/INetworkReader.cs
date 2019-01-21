namespace Lockstep.Framework
{
    public interface INetworkReader
    {
        ushort GetUShort();
        int[] GetIntArray();
        long GetLong();
        int GetInt();
        void SetSource(byte[] data);
        ushort PeekUShort();
        byte GetByte();
        ulong GetULong();
        uint GetUInt();
        byte[] GetBytesWithLength();
    }
}