using Lockstep.Framework.Networking.Messages;

namespace Lockstep.Framework
{
    public interface ICommandHandler
    {
        void Handle(Command command);
    }
}