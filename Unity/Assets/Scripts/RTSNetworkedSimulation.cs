using System.Collections;   
using Lockstep.Client;
using Lockstep.Client.Implementations;
using Lockstep.Client.Interfaces;
using Lockstep.Commands;
using Lockstep.Core;
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

    private bool _simulationStarted;
    private LockstepSystems _systems;
    private NetworkedDataSource _dataSource;

    private void Awake()
    {
        Instance = this;
        _dataSource = new NetworkedDataSource(_client)
            .RegisterCommand(() => new SpawnCommand())
            .RegisterCommand(() => new NavigateCommand());

        _systems = new LockstepSystems(Contexts.sharedInstance, new UnityGameService(EntityDatabase),
            new UnityLogger());

        _simulation =
            new Simulation(_systems, _dataSource);      
            

        _simulation.Started += (sender, args) => _simulationStarted = true;
        _simulation.Ticked += (id, frame) =>
        {
            _dataSource.Receive(MessageTag.HashCode, new HashCode {FrameNumber = id, Value = Contexts.sharedInstance.gameState.hashCode.value});
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
    }

    void FixedUpdate()
    {
        //if (!_simulationStarted)
        //{
        //    return;
        //}   

        //simulation.Simulate(); 

        //if (simulation.FrameCounter % 10 == 0)
        //{
        //    LockstepNetwork.Instance.SendHashCode(new Checksum{FrameNumber = simulation.FrameCounter, Value = simulation.CalculateChecksum()});
        //}                               
    }

    void OnGUI()
    {
        if (_simulationStarted)
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
