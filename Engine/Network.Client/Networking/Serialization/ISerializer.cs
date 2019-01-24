namespace Lockstep.Framework.Networking.Serialization
{
    public interface ISerializer
    {
        byte[] Data { get; }
        int Length { get; }


        void Put(byte value);
        void Put(int value);
        void Put(uint value);
        void Put(long value);
        void Put(ushort value);
        void PutArray(int[] array);
        void Reset();
        void PutBytesWithLength(byte[] data);
        void Put(ulong value);
        void Put(byte[] input);
    }
}