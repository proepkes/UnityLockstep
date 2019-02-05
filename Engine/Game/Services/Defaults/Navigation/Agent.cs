using BEPUutilities;
using FixMath.NET;

namespace Lockstep.Game.Services.Defaults.Navigation
{
    class Agent
    {
        public Vector2 Position;

        public Vector2 Velocity;

        public Vector2 Destination;

        public Agent(Vector2 position)
        {                    
            Position = position;
            Destination = position;
        }

        public void Update()
        {
            Velocity = Destination - Position;
            if (Velocity.LengthSquared() > Fix64.One)
            {
                Velocity.Normalize(); 
            }      
        }
    }
}
