using ECS.Data;                      

namespace ECS.DefaultServices
{                     
    public class DefaultInputService : IInputService
    {       
        public Frame GetNextFrame()
        {
            return new Frame();
        }
    }
}