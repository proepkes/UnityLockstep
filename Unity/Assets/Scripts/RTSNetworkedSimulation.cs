            
using System.Collections;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using FastFileLog;
using Lockstep.Client;
using Lockstep.Client.Commands;
using Lockstep.Client.Implementations;
using Lockstep.Client.Interfaces;
using Lockstep.Core;
using Lockstep.Core.Interfaces;
using Lockstep.Network.Messages;
using Lockstep.Network.Utils;
using Newtonsoft.Json;
using UnityEngine;           
                              
public class RTSNetworkedSimulation : MonoBehaviour
{      
    public static RTSNetworkedSimulation Instance;

    public string ServerIp = "127.0.0.1";
    public int ServerPort = 9050;

    public IWorld Systems;
    public Simulation Simulation;
    public RTSEntityDatabase EntityDatabase;

    public bool Connected => _client.Connected;

    public byte PlayerId { get; private set; }
    public byte[] AllActorIds { get; private set; }

    private NetworkCommandBuffer _remoteCommandBuffer;
    private readonly LiteNetLibClient _client = new LiteNetLibClient();

    private void Awake()
    {                                
        Instance = this;

        Systems = new World(Contexts.sharedInstance, new UnityGameService(EntityDatabase), new UnityLogger());

        _remoteCommandBuffer = new NetworkCommandBuffer(_client);
        _remoteCommandBuffer.RegisterCommand(() => new SpawnCommand());
        _remoteCommandBuffer.RegisterCommand(() => new NavigateCommand());

        Simulation = new Simulation(Systems, _remoteCommandBuffer) { LagCompensation = 3 };

        _remoteCommandBuffer.InitReceived += StartSimulation;   

        //Simulation.Ticked += id =>
        //{
        //    _dataReceiver.Receive(MessageTag.HashCode, new HashCode {FrameNumber = id, Value = _systems.HashCode});
        //};
    }

    private void StartSimulation(Init data)
    {                             
        PlayerId = data.ActorID;
        AllActorIds = data.AllActors;
        Debug.Log($"Starting simulation. Total actors: {data.AllActors.Length}. Local ActorID: {data.ActorID}");
        Simulation.Initialize(data);  

        _remoteCommandBuffer.InitReceived -= StartSimulation;
    }


    public void DumpInputContext()
    {
        var serializer = new Serializer();
        serializer.Put(Contexts.sharedInstance.gameState.hashCode.value);
        serializer.Put(Contexts.sharedInstance.gameState.tick.value);
        serializer.Put(PlayerId);
        serializer.PutBytesWithLength(AllActorIds);
        IFormatter formatter = new BinaryFormatter();

        Stream stream = new FileStream(@"C:\Log\"+ Contexts.sharedInstance.gameState.hashCode.value+".txt", FileMode.Create, FileAccess.Write);
        stream.Write(serializer.Data, 0, serializer.Length);
        formatter.Serialize(stream, Systems.DebugHelper);
        stream.Close();   
    }


    public void Execute(ISerializableCommand command)
    {
        Simulation.Execute(command);
    }

    private void Start()
    {
        _client.Start();
        StartCoroutine(AutoConnect());
    }

    private void OnDestroy()
    {
        _client.Stop();   
    }


    void Update()
    {
        _client.Update();
        Simulation.Update(Time.deltaTime * 1000);
    }            

    public IEnumerator AutoConnect()
    {
        while (!Connected)
        {
            _client.Connect(ServerIp, ServerPort);
            yield return new WaitForSeconds(1);
        }

        yield return null;
    }
}     
