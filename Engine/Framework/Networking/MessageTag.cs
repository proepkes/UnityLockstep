namespace Lockstep.Framework.Networking
{
    public enum MessageTag : byte
    {
        StartSimulation,
        Frame,
        Input,
        Checksum,
    }
}