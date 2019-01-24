using System;

namespace Server
{
    public interface IServer
    {
        event Action<int> ClientConnected;
        event Action<int> ClientDisconnected;

        event Action<int, byte[]> DataReceived;

        void Distribute(byte[] data, int length);

        void Send(int clientId, byte[] data, int length);

        void Run(int port);
    }
}