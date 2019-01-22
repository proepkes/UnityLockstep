using ECS;
using ECS.Data;
using Lockstep.Framework;
using Lockstep.Framework.Commands;
using Lockstep.Framework.Networking.Messages;
using Lockstep.Framework.Networking.Serialization;  
using UnityEngine;           
                              
public class RTSSimulator : MonoBehaviour
{                                                                        
    private Simulation _simulation;      
    private bool _simulationStarted;

    public static RTSSimulator Instance;

    public RTSEntityDatabase EntityDatabase;

    private InputParser _inputParser;
                                                         

    private void Awake()
    {
        Instance = this;

        _simulation = new Simulation(
            Contexts.sharedInstance, 
            new ServiceContainer()                      
                //.Register<INavigationService>(new RVONavigationService())                          
                .Register<IGameService>(new UnityGameService(EntityDatabase))
                .Register<ILogService>(new UnityLogger())) 
            {
                FrameDelay = 2,
            };

        _inputParser = new InputParser(r =>
        {
            ICommand cmd;
            var cmdTag = (CommandTag)r.GetUShort();
            switch (cmdTag)
            {
                case CommandTag.Spawn:
                    cmd = new SpawnCommand();
                    break;
                case CommandTag.Navigate:
                    cmd = new NavigateCommand();
                    break;
                default:
                    return null;
            }
                       
            ((ISerializable) cmd).Deserialize(r);

            return cmd;
        });
    }     

    private void Start()
    {
        LockstepNetwork.Instance.MessageReceived += NetworkOnMessageReceived;
    }

    private void NetworkOnMessageReceived(MessageTag messageTag, IDeserializer reader)
    {
        switch (messageTag)
        {
            case MessageTag.StartSimulation:
                var pkt = new Init();
                pkt.Deserialize(reader);
                Time.fixedDeltaTime = 1f/pkt.TargetFPS;  

       
                 _simulation.Init(pkt.Seed);

                _simulationStarted = true;
                break;
            case MessageTag.Frame:
                //Server sends in ReliableOrdered-mode, so we only care about the latest frame
                //Also possible could be high-frequency unreliable messages and use redundant frames to fill up a framebuffer in case of frame loss during transmission 
                _simulation.AddFrame(new Frame {Commands = _inputParser.DeserializeInput(reader)});

                //TODO: only for debugging, frames should be buffered
                _simulation.Simulate();

                LockstepNetwork.Instance.SendHashCode(new HashCode
                {
                    FrameNumber = _simulation.FrameCounter,
                    Value = _simulation.HashCode
                });   
                break;
        }
    }            
            

    void FixedUpdate()
    {
        if (!_simulationStarted)
        {
            return;
        }   

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
            GUILayout.Label("HashCode: " + _simulation.HashCode);
            GUILayout.EndVertical(); 
        }
    }
}
