namespace ECS.Data
{
    public class Frame
    {                                          
        public SerializedInput[] SerializedInputs { get; set; }     
    }

    public class SerializedInput
    {
        public byte[] Data { get; set; }
    }
}