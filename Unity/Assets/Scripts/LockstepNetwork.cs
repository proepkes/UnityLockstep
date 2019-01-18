using System;   
using LiteNetLib;
using LiteNetLib.Utils;
using Lockstep.Framework.Commands;
using Lockstep.Framework.Networking;
using Lockstep.Framework.Networking.Serialization;   
using UnityEngine;   

public class LockstepNetwork : MonoBehaviour
{
    public static LockstepNetwork Instance;

    public event Action<MessageTag, NetDataReader> MessageReceived;

    public bool DrawGUI;            

    public string IP = "127.0.0.1";             

    NetManager client;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        var listener = new EventBasedNetListener();  
        listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod) =>
        {      
            MessageReceived?.Invoke((MessageTag)dataReader.GetByte(), dataReader);   
            dataReader.Recycle();
        };

        client = new NetManager(listener);
        client.Start();
    }
                                        
    void Update()
    {
        client.PollEvents();   
    }

    private void OnDestroy()
    {
        client.Stop();       
    }     

    public void SendChecksum(Checksum checksum)
    {
        var writer = new NetDataWriter();
        writer.Put((byte)MessageTag.Checksum);
        checksum.Serialize(writer);   
        client.FirstPeer.Send(writer, DeliveryMethod.ReliableOrdered);
    }

    public void SendInput(ISerilalizableCommand message)
    {
        var writer = new NetDataWriter();
        writer.Put((byte) MessageTag.Input);                                       
        message.Serialize(writer);
        Debug.Log(writer.Length + " bytes");
        client.FirstPeer.Send(writer, DeliveryMethod.ReliableOrdered);
    }
               
    void OnGUI()
    {
        if (!DrawGUI)
        {
            return;
        }                                                                 

        if (client.FirstPeer?.ConnectionState == LiteNetLib.ConnectionState.Connected)
        {
            return;
        }

        GUILayout.BeginVertical(GUILayout.Width(300f));
        GUILayout.Label("Time: " + Time.time);
        GUI.color = Color.white;
        GUILayout.Label("IP: ");
        IP = GUILayout.TextField(IP);
                                  
        if (GUILayout.Button("Connect"))
        {                                     
            client.Start();
            client.Connect(IP, 9050 /* port */, "SomeConnectionKey" /* text key or NetDataWriter */);
        }
        GUILayout.EndVertical();
    }
}
