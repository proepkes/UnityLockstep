using System;
using System.Linq;
using LiteNetLib;
using Lockstep.Network.Server.Interfaces;

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
            _server = new NetManager(_listener)
            {
                DisconnectTimeout = 30000
            };
        }

        public void Distribute(byte[] data)
        {
            _server.SendToAll(data, DeliveryMethod.ReliableOrdered);
        }

        public void Distribute(int clientId, byte[] data)
        {
            _server.SendToAll(data, DeliveryMethod.ReliableOrdered, _server.ConnectedPeerList.First(peer => peer.Id == clientId));
        }

        public void Send(int clientId, byte[] data)
        {
            _server.ConnectedPeerList.First(peer => peer.Id == clientId).Send(data, DeliveryMethod.ReliableOrdered);
        }

        public void Start(int port)
        {
            _listener.ConnectionRequestEvent += OnConnectionRequest;
            _listener.PeerConnectedEvent += OnPeerConnected;
            _listener.NetworkReceiveEvent += OnNetworkReceive;
            _listener.PeerDisconnectedEvent += OnPeerDisconnected;
            _server.Start(port);
        }

        private void OnPeerDisconnected(NetPeer peer, DisconnectInfo info)
        {
            ClientDisconnected?.Invoke(peer.Id);
        }

        private void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod method)
        {
            DataReceived?.Invoke(peer.Id, reader.GetRemainingBytes());
        }

        private void OnConnectionRequest(ConnectionRequest request)
        {
            request.AcceptIfKey(ClientKey);
        }

        private void OnPeerConnected(NetPeer peer)
        {
            ClientConnected?.Invoke(peer.Id);
        }

        public void Stop()
        {
            _listener.ConnectionRequestEvent -= OnConnectionRequest;
            _listener.PeerConnectedEvent -= OnPeerConnected;
            _listener.NetworkReceiveEvent -= OnNetworkReceive;
            _listener.PeerDisconnectedEvent -= OnPeerDisconnected;
            _server.Stop();
        }

        public void PollEvents()
        {
            _server.PollEvents();
        }
    }
}
