            
using System.Collections;              
using System.IO;                
using Lockstep.Game;
using Lockstep.Game.Commands;          
using Lockstep.Game.Network;
using Lockstep.Game.Simulation;
using Lockstep.Network.Utils;         
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

    private NetworkCommandBuffer _remoteCommandBuffer;
    private readonly LiteNetLibClient _client = new LiteNetLibClient();

    private void Awake()
    {                                
        Instance = this;

        _remoteCommandBuffer = new NetworkCommandBuffer(_client);
        _remoteCommandBuffer.RegisterCommand(() => new SpawnCommand());
        _remoteCommandBuffer.RegisterCommand(() => new NavigateCommand());

        Simulation = new Bootstrapper(_client, 3, new UnityGameService(EntityDatabase), new UnityLogger()).Simulation;
        Simulation.Started += (sender, args) =>
        {
            Debug.Log($"Starting simulation. Total actors: {Simulation.AllActorIds.Length}. Local ActorID: {Simulation.LocalActorId}"); 
        };        
    }             


    public void DumpInputContext()
    {
        Stream stream = new FileStream(@"C:\Log\" + Simulation.LocalActorId + "_" + Contexts.sharedInstance.gameState.hashCode.value + "_log.txt", FileMode.Create, FileAccess.Write);
        var serializer = new Serializer();
        serializer.Put(Contexts.sharedInstance.gameState.hashCode.value);
        serializer.Put(Contexts.sharedInstance.gameState.tick.value);
        serializer.Put(Simulation.LocalActorId);
        serializer.PutBytesWithLength(AllActorIds);
        stream.Write(serializer.Data, 0, serializer.Length);

        Simulation.GameLog.WriteTo(stream);

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
