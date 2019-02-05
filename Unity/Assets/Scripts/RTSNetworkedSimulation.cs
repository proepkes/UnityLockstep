            
using System.Collections;              
using System.IO;                
using Lockstep.Game;
using Lockstep.Game.Commands;          
using Lockstep.Game.Networking;
using Lockstep.Network.Messages;
using Lockstep.Network.Utils;         
using UnityEngine;           
                              
public class RTSNetworkedSimulation : MonoBehaviour
{      
    public static RTSNetworkedSimulation Instance;

    public string ServerIp = "127.0.0.1";
    public int ServerPort = 9050;

    public World Systems;           
    public RTSEntityDatabase EntityDatabase;

    public bool Connected => _client.Connected;

    public byte PlayerId { get; private set; }
    public byte[] AllActorIds { get; private set; }

    private NetworkCommandBuffer _remoteCommandBuffer;
    private readonly LiteNetLibClient _client = new LiteNetLibClient();

    private void Awake()
    {                                
        Instance = this;

        _remoteCommandBuffer = new NetworkCommandBuffer(_client);
        _remoteCommandBuffer.RegisterCommand(() => new SpawnCommand());
        _remoteCommandBuffer.RegisterCommand(() => new NavigateCommand());
        Systems = new World(Contexts.sharedInstance, _remoteCommandBuffer, new UnityGameService(EntityDatabase), new UnityLogger())
        {
            LagCompensation = 3
        };  
                                                                       

        _remoteCommandBuffer.InitReceived += StartSimulation;  
    }

    private void StartSimulation(Init data)
    {                             
        PlayerId = data.ActorID;
        AllActorIds = data.AllActors;
        Debug.Log($"Starting simulation. Total actors: {data.AllActors.Length}. Local ActorID: {data.ActorID}");
        Systems.Initialize(data);  

        _remoteCommandBuffer.InitReceived -= StartSimulation;
    }


    public void DumpInputContext()
    {
        Stream stream = new FileStream(@"C:\Log\" + PlayerId + "_" + Contexts.sharedInstance.gameState.hashCode.value + "_log.txt", FileMode.Create, FileAccess.Write);
        var serializer = new Serializer();
        serializer.Put(Contexts.sharedInstance.gameState.hashCode.value);
        serializer.Put(Contexts.sharedInstance.gameState.tick.value);
        serializer.Put(PlayerId);
        serializer.PutBytesWithLength(AllActorIds);
        stream.Write(serializer.Data, 0, serializer.Length);

        Systems.GameLog.WriteTo(stream);

        stream.Close();                    
    }


    public void Execute(ISerializableCommand command)
    {
        Systems.Execute(command);
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
        Systems.Update(Time.deltaTime * 1000);
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
