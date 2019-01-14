namespace Lockstep.Framework.Networking
{
    public enum MessageTag : byte
    {
        Init,
        Frame,
        Command,
        Checksum,
    }
}