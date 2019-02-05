using System;

namespace Lockstep.Network.Server
{
    public interface IServer
    {
        event Action<int> ClientConnected;
        event Action<int> ClientDisconnected;

        event Action<int, byte[]> DataReceived;

        void Distribute(byte[] data);

        void Distribute(int sourceClientId, byte[] data);

        void Send(int clientId, byte[] data);

        void Run(int port);
    }
}