using System;
using System.Collections;
using LiteNetLib;
using Lockstep.Framework.Commands;
using Lockstep.Framework.Networking.Messages;
using Lockstep.Framework.Networking.Serialization;
using Server.LiteNetLib;
using UnityEngine;   

public class LockstepNetwork : MonoBehaviour
{
    public static LockstepNetwork Instance;

    public event Action<MessageTag, IDeserializer> MessageReceived;        

    public string IP;

    private NetManager _client;

    public bool Connected => _client.FirstPeer?.ConnectionState == ConnectionState.Connected;

    private void Awake()
    {
        Instance = this;

        var listener = new EventBasedNetListener();
        listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod) =>
        {
            MessageReceived?.Invoke((MessageTag)dataReader.GetByte(), new LiteNetLibDeserializer(dataReader));
            dataReader.Recycle();
        };

        _client = new NetManager(listener);
    }

    void Start()
    {
        _client.Start();
        StartCoroutine(Connect());
    }
                                        
    void Update()
    {          
        _client.PollEvents();   
    }

    private void OnDestroy()
    {
        _client.Stop();       
    }     

    public void SendHashCode(HashCode checksum)
    {
        var writer = new LiteNetLibSerializer();
        writer.Put((byte)MessageTag.Checksum);
        checksum.Serialize(writer);   
        _client.FirstPeer.Send(writer.Data, 0, writer.Length, DeliveryMethod.ReliableOrdered);
    }

    public void SendInput(CommandTag commandTag, ISerializable message)
    {
        var writer = new LiteNetLibSerializer();
        writer.Put((byte) MessageTag.Input); 
        writer.Put((ushort)commandTag);
        message.Serialize(writer);

        Debug.Log(writer.Length + " bytes");
        _client.FirstPeer.Send(writer.Data, 0, writer.Length, DeliveryMethod.ReliableOrdered);
    }

    public IEnumerator Connect()
    {
        while (!Connected)
        {
            _client.Connect(IP, 9050 /* port */, "SomeConnectionKey" /* text key or NetDataWriter */);
            yield return new WaitForSeconds(1);
        }

        yield return null;
    }          
}
