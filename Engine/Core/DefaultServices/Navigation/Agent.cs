using BEPUutilities;
using FixMath.NET;

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
            var goal = Destination - Position;
            if (goal.LengthSquared() > Fix64.One)
            {
                goal = Vector2.Normalize(goal);
            }
            Position += goal;       
        }
    }
}
