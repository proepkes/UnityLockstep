using System.Collections;   
using Lockstep.Client;
using Lockstep.Client.Implementations;
using Lockstep.Client.Interfaces;
using Lockstep.Commands;
using Lockstep.Core;
using Lockstep.Core.Interfaces;
using Lockstep.Network;
using Lockstep.Network.Messages;
using UnityEngine;           
                              
public class RTSNetworkedSimulation : MonoBehaviour
{
    public static RTSNetworkedSimulation Instance;
                                        
    private Simulation _simulation;
    private readonly LiteNetLibClient _client = new LiteNetLibClient(); 
                                           
    public RTSEntityDatabase EntityDatabase;

    public bool Connected => _client.Connected;

    public string ServerIp = "127.0.0.1";
    public int ServerPort = 9050;
                                       
    private LockstepSystems _systems;
    private NetworkedDataReceiver _dataReceiver;

    private ILogService logger;

    private void Awake()
    {
        logger = new UnityLogger();
        Instance = this;

        _dataReceiver = new NetworkedDataReceiver(_client)
            .RegisterCommand(() => new SpawnCommand())
            .RegisterCommand(() => new NavigateCommand());

        _systems = new LockstepSystems(Contexts.sharedInstance, new UnityGameService(EntityDatabase));

        _simulation =
            new Simulation(_systems, _dataReceiver);      
                                                                             
        _simulation.Ticked += id =>
        {
            _dataReceiver.Receive(MessageTag.HashCode, new HashCode {FrameNumber = id, Value = _systems.HashCode});
        };
    }


    public void Execute(ISerializableCommand command)
    {
        _simulation.Execute(command);
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

        _simulation.Update(Time.deltaTime * 1000, logger);
    }   

    void OnGUI()
    {
        if (_simulation.Running)
        {
            GUILayout.BeginVertical(GUILayout.Width(100f));
            GUI.color = Color.white;
            GUILayout.Label("HashCode: " + Contexts.sharedInstance.gameState.hashCode.value);
            GUILayout.EndVertical();
        }
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
