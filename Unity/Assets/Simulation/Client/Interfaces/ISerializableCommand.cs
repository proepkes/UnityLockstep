using Lockstep.Core.Interfaces;
using Lockstep.Network;

namespace Lockstep.Client.Interfaces
{
    public interface ISerializableCommand  : ISerializable, ICommand
    {
        ushort Tag { get; }   
    }
}