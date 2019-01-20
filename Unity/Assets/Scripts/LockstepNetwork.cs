using System;
using System.Collections;
using LiteNetLib;
using LiteNetLib.Utils;
using Lockstep.Framework.Commands;
using Lockstep.Framework.Networking;
using Lockstep.Framework.Networking.Serialization;
using TMPro;
using UnityEngine;   

public class LockstepNetwork : MonoBehaviour
{
    public static LockstepNetwork Instance;

    public event Action<MessageTag, NetDataReader> MessageReceived;        

    public string IP;

    private NetManager _client;

    public bool Connected => _client.FirstPeer?.ConnectionState == ConnectionState.Connected;

    private void Awake()
    {
        Instance = this;

        var listener = new EventBasedNetListener();
        listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod) =>
        {
            MessageReceived?.Invoke((MessageTag)dataReader.GetByte(), dataReader);
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
        var writer = new NetDataWriter();
        writer.Put((byte)MessageTag.Checksum);
        checksum.Serialize(writer);   
        _client.FirstPeer.Send(writer, DeliveryMethod.ReliableOrdered);
    }

    public void SendInput(ISerilalizableCommand message)
    {
        var writer = new NetDataWriter();
        writer.Put((byte) MessageTag.Input);                                       
        message.Serialize(writer);
        Debug.Log(writer.Length + " bytes");
        _client.FirstPeer.Send(writer, DeliveryMethod.ReliableOrdered);
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
