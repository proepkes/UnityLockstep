using System;
using Lockstep.Client.Interfaces;
using Lockstep.Core.Data;
using Lockstep.Network.Messages;

namespace Lockstep.Client.Implementations
{
    public class LocalDataReceiver : IDataReceiver
    {
        public event EventHandler<Init> InitReceived;
        public event EventHandler<Frame> FrameReceived;

        public void Init(byte playerId, int seed, int targetFps)
        {
            InitReceived?.Invoke(this, new Init { PlayerID = playerId, Seed = seed, TargetFPS = targetFps });
        }

        public void Receive(Frame frame)
        {             
            FrameReceived?.Invoke(this, frame);
        }

        public void Receive(ICommand command)
        {                                       
            FrameReceived?.Invoke(this, new Frame{Commands = new []{command}});
        }
    }
}
