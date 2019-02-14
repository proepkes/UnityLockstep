using System;

namespace Lockstep.Network.Server.Interfaces
{
    public interface IServer
    {
        event Action<int> ClientConnected;
        event Action<int> ClientDisconnected;

        event Action<int, byte[]> DataReceived;

        /// <summary>
        /// Send data to every connected client
        /// </summary>
        /// <param name="data">The data</param>
        void Distribute(byte[] data);

        /// <summary>
        /// Send data to every connected client except one
        /// </summary>
        /// <param name="sourceClientId">The id of the excluded client</param>
        /// <param name="data">The data</param>
        void Distribute(int sourceClientId, byte[] data);

        /// <summary>
        /// Send data to a specific client
        /// </summary>
        /// <param name="clientId">The id of the specific client</param>
        /// <param name="data">The data</param>
        void Send(int clientId, byte[] data);

        /// <summary>
        /// Begins listening for client on a specific port
        /// </summary>
        /// <param name="port">The port to listen on</param>
        void Run(int port);
    }
}