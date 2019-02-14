            
using System;
using System.Collections;              
using System.IO;
using Lockstep.Common.Logging;
using Lockstep.Core.Logic.Interfaces;
using Lockstep.Core.Logic.Serialization.Utils;
using Lockstep.Game;                      
using Lockstep.Network.Client;
using UnityEngine;           
                              
public class RTSNetworkedSimulation : MonoBehaviour
{      
    public static RTSNetworkedSimulation Instance;

    public string ServerIp = "127.0.0.1";
    public int ServerPort = 9050;

    public Simulation Simulation;           
    public RTSEntityDatabase EntityDatabase;

    public bool Connected => _client.Connected;       

    public byte[] AllActorIds { get; private set; }

    private NetworkCommandQueue _commandQueue;
    private readonly LiteNetLibClient _client = new LiteNetLibClient();

    private void Awake()
    {                                
        Instance = this;

        Log.OnMessage += (sender, args) => Debug.Log(args.Message);

        _commandQueue = new NetworkCommandQueue(_client)
        {
            LagCompensation = 10
        };
        _commandQueue.InitReceived += (sender, init) =>
        {
            AllActorIds = init.AllActors;
            Debug.Log($"Starting simulation. Total actors: {init.AllActors.Length}. Local ActorID: {init.ActorID}");
            Simulation.Start(init.SimulationSpeed, init.ActorID, init.AllActors);
        };

        Simulation = new Simulation(Contexts.sharedInstance, _commandQueue, new UnityGameService(EntityDatabase));      
    }             


    public void DumpGameLog()
    {
        Simulation.DumpGameLog(new FileStream(@"C:\Log\" + Math.Abs(Contexts.sharedInstance.gameState.hashCode.value) + ".bin", FileMode.Create, FileAccess.Write));                   
    }

    public void Execute(ICommand command)
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
