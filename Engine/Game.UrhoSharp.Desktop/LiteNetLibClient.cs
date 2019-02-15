using System;
using LiteNetLib;              
using Lockstep.Network.Client;   

namespace Game.UrhoSharp.Desktop
{
    public class LiteNetLibNetwork : INetwork
    {
        private readonly EventBasedNetListener _listener = new EventBasedNetListener();

        private NetManager _client;

        public event Action<byte[]> DataReceived;

        public bool Connected => _client.FirstPeer?.ConnectionState == ConnectionState.Connected;

        public void Start()
        {
            _listener.NetworkReceiveEvent += OnNetworkReceive;

            _client = new NetManager(_listener)
            {
                DisconnectTimeout = 30000
            };
            
            _client.Start();
        }

        private void OnNetworkReceive(NetPeer fromPeer, NetPacketReader dataReader, DeliveryMethod deliveryMethod)
        {
            DataReceived?.Invoke(dataReader.GetRemainingBytes());
            dataReader.Recycle();
        }

        public void Connect(string serverIp, int port)
        {
            _client.Connect(serverIp, port, "SomeConnectionKey");
        }

        public void Send(byte[] data)
        {
            _client.FirstPeer.Send(data, DeliveryMethod.ReliableOrdered);
        }

        public void Update()
        {
            _client.PollEvents();
        }

        public void Stop()
        {
            _listener.NetworkReceiveEvent -= OnNetworkReceive;
            _client.Stop();
        }
    }
}
