using Client;
using ECS;
using Lockstep.Client;
using Lockstep.Commands;
using Lockstep.Core;        
using Lockstep.Core.Interfaces;       
using UnityEngine;           
                              
public class RTSSimulator : MonoBehaviour
{                                        
    //private bool _simulationStarted;

    public static RTSSimulator Instance;
    public NetworkedSimulation Simulation;

    public RTSEntityDatabase EntityDatabase;

    public string IP;

    private readonly LiteNetLibClient _client = new LiteNetLibClient();

    private void Awake()
    {
        Instance = this;
        Simulation = new NetworkedSimulation(
            _client,
            new LockstepSystems(
                Contexts.sharedInstance, 
                new ServiceContainer()
                    //.Register<INavigationService>(new RVONavigationService())                          
                    .Register<IGameService>(new UnityGameService(EntityDatabase))
                    .Register<ILogService>(new UnityLogger()), 
                new FrameDataSource()))
            .RegisterCommand(() => new SpawnCommand())
            .RegisterCommand(() => new NavigateCommand());     
    }     

    private void Start()
    {
        _client.Start();
        Simulation.Start(IP, 9050);
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
        //if (_simulationStarted)
        //{
        //    GUILayout.BeginVertical(GUILayout.Width(100f));
        //    GUI.color = Color.white;
        //    GUILayout.Label("HashCode: " + Simulation.HashCode);
        //    GUILayout.EndVertical(); 
        //}
    }     
}
