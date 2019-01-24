using System;

namespace Client
{
    public interface IClient
    {
        event Action<byte[]> DataReceived; 

        void Connect(string ipAddress, int port);

        /// <summary>
        /// Send data reliable ordered
        /// </summary>
        /// <param name="data"></param>
        void Send(byte[] data, int length);
    }
}