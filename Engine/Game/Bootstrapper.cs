using Lockstep.Core.Commands;
using Lockstep.Core.Services;
using Lockstep.Game.Commands;
using Lockstep.Game.Networking;
using Lockstep.Network;
using Lockstep.Network.Messages;
using Lockstep.Network.Utils;

namespace Lockstep.Game
{
    public class Bootstrapper : IMessageHandler
    {
        private readonly Client _client;

        public Simulation Simulation { get; }

        public Bootstrapper(params IService[] additionalServices) : this(Contexts.sharedInstance, additionalServices)
        {                                                                                                   
        }

        public Bootstrapper(Contexts contexts, params IService[] additionalServices) : this(Contexts.sharedInstance, new CommandBuffer(), additionalServices)
        {                                                                                    
        }

        public Bootstrapper(Contexts contexts, ICommandBuffer commandBuffer, params IService[] additionalServices)
        {
            Simulation = new Simulation(contexts, commandBuffer, additionalServices);
        }

        public Bootstrapper(INetwork network, uint lagCompensation, params IService[] additionalServices)
        {             
            var commandBuffer = new NetworkCommandBuffer(network)
            {
                LagCompensation = lagCompensation
            };
            commandBuffer.RegisterCommand(() => new NavigateCommand());
            commandBuffer.RegisterCommand(() => new SpawnCommand());

            _client = new Client(network);
            _client.AddHandler(this, MessageTag.Init);
            _client.AddHandler(commandBuffer, MessageTag.Input);

            Simulation = new Simulation(Contexts.sharedInstance, commandBuffer, additionalServices);
        }

        public void Handle(MessageTag tag, byte[] data)
        {             
            var reader = new Deserializer(data);      

            switch (tag)
            {
                case MessageTag.Init:                             
                    var init = new Init();
                    init.Deserialize(reader);
                    StartSimulation(init);
                    break;
            }
        }

        public void StartSimulation(Init init)
        {
            StartSimulation(init.TargetFPS, init.ActorID, init.AllActors );
        }              

        public void StartSimulation(int targetFps, byte localActorId, byte[] allActors)
        {
            Simulation.Start(targetFps, localActorId, allActors);
        }
    }
}
