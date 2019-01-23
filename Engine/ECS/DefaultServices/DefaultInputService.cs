using ECS.Data;                      

namespace ECS.DefaultServices
{                     
    public class DefaultDataSource : IDataSource
    {       
        public Frame GetNextFrame()
        {
            return new Frame();
        }
    }
}