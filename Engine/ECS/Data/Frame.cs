namespace ECS.Data
{
    public class Frame
    {                                               
        public uint FrameNumber { get; set; }
        public Command[] Commands { get; set; }   
    }     
}