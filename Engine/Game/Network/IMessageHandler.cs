using Lockstep.Network;

namespace Lockstep.Game.Network
{
    public interface IMessageHandler
    {
        void Handle(MessageTag tag, byte[] data);
    }
}