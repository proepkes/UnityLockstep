using System;

namespace Client
{
    public interface IClient
    {
        event Action<byte[]> DataReceived; 

        /// <summary>
        /// Connect to the server. Connectiondetails like server-ip and port have to be handled by the implementation
        /// </summary>
        void Connect();

        /// <summary>
        /// Send data reliable ordered
        /// </summary>
        /// <param name="data"></param>
        void Send(byte[] data, int length);
    }
}