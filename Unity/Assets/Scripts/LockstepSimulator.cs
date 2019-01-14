using LiteNetLib.Utils;
using Lockstep.Framework;
using Lockstep.Framework.Abilities;
using Lockstep.Framework.Networking;
using Lockstep.Framework.Networking.Messages;        
using UnityEngine;                            

public class LockstepSimulator : MonoBehaviour, ICommandHandler
{
    private readonly NetDataReader commandReader = new NetDataReader();

    private Simulation simulation;      
    private bool simulationStarted;

    private void Awake()
    {
        simulation = new Simulation(this)
        {
            FrameDelay = 2,
        };
    }     

    private void Start()
    {
        LockstepNetwork.Instance.MessageReceived += NetworkOnMessageReceived;
    }

    private void NetworkOnMessageReceived(MessageTag messageTag, NetDataReader reader)
    {
        switch (messageTag)
        {
            case MessageTag.Init:
                var pkt = new Init();
                pkt.Deserialize(reader);
                Time.fixedDeltaTime = 1f/pkt.TargetFPS;

                simulation.SetSeed(pkt.Seed);

                simulationStarted = true;
                break;
            case MessageTag.Frame:
                //Server sends in ReliableOrdered-mode, so we only care about the latest frame
                //Also possible could be high-frequency unreliable messages and use the redundant frames to fill up the framebuffer in case a frame was lost during transmission
                var pkg = new FramePackage(1);
                pkg.Deserialize(reader);

                simulation.AddFrame(pkg.Frames[0]); 
                break;
        }
    }        

    public void Handle(Command command)
    {
        commandReader.SetSource(command.Data);
        HandleCommand((CommandTag)commandReader.GetUShort(), commandReader);
    }

    private void HandleCommand(CommandTag commandTag, NetDataReader reader)
    {
        switch (commandTag)
        {
            case CommandTag.Move:
                var pkt = new MovePacket();
                pkt.Deserialize(reader);

                var d = new BEPUutilities.Vector2(pkt.PosX, pkt.PosY);                  

                foreach (LockstepAgent entity in simulation.GetEntities())
                {
                    entity.GetAbility<Move>().StartMove(d);
                }
                break;
        }
    }

    public void RegisterEntity(ILockstepEntity entity)
    {
        simulation.EnqueueEntity(entity);
    }
                             

    void FixedUpdate()
    {
        if (!simulationStarted)
        {
            return;
        }   

        simulation.Simulate(); 

        if (simulation.FrameCounter % 10 == 0)
        {
            LockstepNetwork.Instance.SendChecksum(new Checksum{FrameNumber = simulation.FrameCounter, Value = simulation.CalculateChecksum()});
        }                               
    }     
}
