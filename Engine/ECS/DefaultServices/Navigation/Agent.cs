using BEPUutilities;

namespace ECS.DefaultServices.Navigation
{
    class Agent
    {
        public Vector2 Position;

        public Vector2 Destination;

        public Agent(Vector2 position)
        {                    
            Position = position;
            Destination = position;
        }

        public void Update()
        {          
            var goal = Vector2.Normalize(Destination - Position);
            Position += Position + goal;       
        }
    }
}
