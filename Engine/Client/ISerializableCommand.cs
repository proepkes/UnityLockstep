using Lockstep.Core.Data;
using Lockstep.Network;

namespace Lockstep.Client
{
    public interface ISerializableCommand  : ISerializable, ICommand
    {
        ushort Tag { get; }   
    }
}