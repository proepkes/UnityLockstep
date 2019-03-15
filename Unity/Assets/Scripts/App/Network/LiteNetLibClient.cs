using LiteNetLib;
using UnityEngine;

public class LiteNetLibClient : MonoBehaviour, INetwork
{
    readonly EventBasedNetListener listener = new EventBasedNetListener();
    NetManager client;

    void Start()
    {
        client = new NetManager(listener);
        client.Start();
        client.Connect("localhost" /* host ip or name */, 9050 /* port */, "SomeConnectionKey" /* text key or NetDataWriter */);
        listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod) =>
        {
            Debug.Log("We got: " + dataReader.GetString(100 /* max length of string */));
            dataReader.Recycle();
        };
    }

    // Update is called once per frame
    void Update()
    {
        client.PollEvents();
    }

    private void OnDestroy()
    {            
        client.Stop();
    }         

    public void Send(byte[] data)
    {
        if (client.FirstPeer == null)
        {
            client.Connect("localhost" /* host ip or name */, 9050 /* port */, "SomeConnectionKey" /* text key or NetDataWriter */);
        }
        if (client.FirstPeer.ConnectionState == ConnectionState.Connected)
        {
            client.FirstPeer.Send(data, DeliveryMethod.ReliableOrdered);
        }
    }
}
