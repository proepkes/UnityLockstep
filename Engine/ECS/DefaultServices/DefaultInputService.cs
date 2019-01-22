using ECS.Data;                      

namespace ECS.DefaultServices
{                     
    public class DefaultInputService : IInputService
    {       
        public Frame ReadNextFrame()
        {
            return new Frame();
        }
    }
}