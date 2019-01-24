using System;
using Client;
using LiteNetLib;   

public class LiteNetLibClient : IClient
{

    EventBasedNetListener listener = new EventBasedNetListener();
    NetManager client;

    public bool Connected => client.FirstPeer?.ConnectionState == ConnectionState.Connected;

    public void Send(byte[] data, int length)
    {
        client.FirstPeer.Send(data, 0, length, DeliveryMethod.ReliableOrdered);
    }

    public event Action<byte[]> DataReceived;
                                                       
    public void Start()
    {
        listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod) =>
        {
            DataReceived?.Invoke(dataReader.GetRemainingBytes());
            dataReader.Recycle();
        };

        client = new NetManager(listener);
        client.Start();   
    }

    public void Update()
    {    
        client.PollEvents();
    }

    public void Connect(string ipAddress, int port)
    {
        client.Connect("localhost" /* host ip or name */, 9050 /* port */, "SomeConnectionKey" /* text key or NetDataWriter */);
    }   
}
