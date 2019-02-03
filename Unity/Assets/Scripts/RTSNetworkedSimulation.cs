            
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Entitas;
using FastFileLog;
using Lockstep.Client;
using Lockstep.Client.Commands;
using Lockstep.Client.Implementations;
using Lockstep.Client.Interfaces;
using Lockstep.Core;
using Lockstep.Core.Interfaces;
using Lockstep.Network.Messages;
using Lockstep.Network.Utils;         
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
                                                          
        Stream stream = new FileStream(@"C:\Log\"+ PlayerId + "_"+ Contexts.sharedInstance.gameState.hashCode.value+"_log.txt", FileMode.Create, FileAccess.Write);
        stream.Write(serializer.Data, 0, serializer.Length);
        formatter.Serialize(stream, Systems.GameLog);
        stream.Close();

                                                                           
        var data = new List<string>();
        foreach (var entity in Contexts.sharedInstance.game.GetEntities(GameMatcher.AllOf(GameMatcher.Id, GameMatcher.ActorId)).OrderBy(entity => entity.actorId.value).ThenBy(entity => entity.id.value))
        {   
            data.Add(entity.actorId.value.ToString());
            data.Add(entity.id.value.ToString());
            data.Add(entity.position.value.ToString());
            data.Add("----------");
        }
        File.WriteAllLines(@"C:\Log\" + PlayerId + "_" + Contexts.sharedInstance.gameState.hashCode.value + "_Ents.txt", data);


        data = new List<string>();
        foreach (var entity in Contexts.sharedInstance.input.GetEntities())
        {
            data.Add(entity.actorId.value.ToString());
            data.Add(entity.tick.value.ToString());
            data.Add(entity.hasCoordinate.ToString());
            data.Add(entity.hasSelection.ToString());
            data.Add("----------");
        }
        File.WriteAllLines(@"C:\Log\" + PlayerId + "_" + Contexts.sharedInstance.gameState.hashCode.value + "_Input.txt", data);
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
