using Lockstep.Network;

namespace Lockstep.Game.Networking
{
    public interface IMessageHandler
    {
        void Handle(MessageTag tag, byte[] data);
    }
}