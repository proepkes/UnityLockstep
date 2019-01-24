using System;

namespace Lockstep.Client
{
    public interface IClient
    {
        bool Connected { get; }

        event Action<byte[]> DataReceived;  

        /// <summary>
        /// Sends data reliable ordered
        /// </summary>
        /// <param name="data"></param>
        /// <param name="length"></param>
        void Send(byte[] data, int length);   
    }
}