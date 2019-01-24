using System;
using System.Linq;
using LiteNetLib;

namespace Server.LiteNetLib
{
    public class LiteNetLibServer : IServer
    {
        private const string ClientKey = "SomeConnectionKey";

        public event Action<int> ClientConnected;
        public event Action<int> ClientDisconnected;
        public event Action<int, byte[]> DataReceived;

        private readonly NetManager _server;
        private readonly EventBasedNetListener _listener;

        public LiteNetLibServer()
        {
            _listener = new EventBasedNetListener();
            _server = new NetManager(_listener);
        }
                        
        public void Distribute(byte[] data, int length)
        {
            _server.SendToAll(data, 0, length, DeliveryMethod.ReliableOrdered);
        }

        public void Send(int clientId, byte[] data, int length)
        {
            _server.ConnectedPeerList.First(peer => peer.Id == clientId).Send(data, 0, length, DeliveryMethod.ReliableOrdered);
        }

        public void Run(int port)
        {
            _listener.ConnectionRequestEvent += request =>
            {       
                request.AcceptIfKey(ClientKey);      
            };

            _listener.PeerConnectedEvent += peer =>
            {
                ClientConnected?.Invoke(peer.Id);
            };

            _listener.NetworkReceiveEvent += (peer, reader, method) =>
            {
                DataReceived?.Invoke(peer.Id, reader.GetRemainingBytes());
            };

            _listener.PeerDisconnectedEvent += (peer, info) =>
            {
                ClientDisconnected?.Invoke(peer.Id);
            };

            _server.Start(port);
        }

        public void PollEvents()
        {
            _server.PollEvents();
        }
    }
}
