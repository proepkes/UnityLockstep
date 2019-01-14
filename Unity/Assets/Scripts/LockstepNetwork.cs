using System;   
using LiteNetLib;
using LiteNetLib.Utils;
using Lockstep;
using Lockstep.Framework.Networking;
using Lockstep.Framework.Networking.Messages;        
using UnityEngine;

[Flags]
public enum CommandTag : ushort
{
    None,
    Move
}

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

    public void SendCommand(CommandTag tag, ICommandPacket message)
    {
        var writer = new NetDataWriter();
        writer.Put((byte) MessageTag.Command);
        writer.Put((ushort) tag);                                         
        message.Serialize(writer);
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
