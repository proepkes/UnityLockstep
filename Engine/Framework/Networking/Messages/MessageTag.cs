namespace Lockstep.Framework.Networking.Messages
{
    public enum MessageTag : byte
    {
        StartSimulation,
        Frame,
        Input,
        Checksum,
    }
}