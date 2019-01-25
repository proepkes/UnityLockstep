using Lockstep.Core.Data;
using Lockstep.Network;

namespace Lockstep.Client.Interfaces
{
    public interface ISerializableCommand  : ISerializable, ICommand
    {
        ushort Tag { get; }   
    }
}