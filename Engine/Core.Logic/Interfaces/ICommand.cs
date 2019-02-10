using Lockstep.Core.Logic.Serialization;

namespace Lockstep.Core.Logic.Interfaces
{
    public interface ICommand : ISerializable
    {
        ushort Tag { get; }

        void Execute(InputEntity inputEntity);
    }
}