using LiteNetLib.Utils;
using Lockstep.Framework.Networking;

namespace Server.LiteNetLib
{
    public class LiteNetLibNetworkWriter : INetworkWriter
    {
        private readonly NetDataWriter _networkWriterImplementation;

        public LiteNetLibNetworkWriter(NetDataWriter networkWriterImplementation = null)
        {
            _networkWriterImplementation = networkWriterImplementation ?? new NetDataWriter();
        }

        public byte[] Data => _networkWriterImplementation.Data;

        public int Length => _networkWriterImplementation.Length;

        public void Put(byte value)
        {
            _networkWriterImplementation.Put(value);
        }

        public void Put(int value)
        {
            _networkWriterImplementation.Put(value);
        }

        public void Put(uint value)
        {
            _networkWriterImplementation.Put(value);
        }

        public void Put(long value)
        {
            _networkWriterImplementation.Put(value);
        }

        public void Put(ushort value)
        {
            _networkWriterImplementation.Put(value);
        }

        public void PutArray(int[] array)
        {
            _networkWriterImplementation.PutArray(array);
        }

        public void Reset()
        {
            _networkWriterImplementation.Reset();
        }

        public void PutBytesWithLength(byte[] data)
        {
            _networkWriterImplementation.PutBytesWithLength(data);
        }

        public void Put(ulong value)
        {
            _networkWriterImplementation.Put(value);
        }
    }
}
