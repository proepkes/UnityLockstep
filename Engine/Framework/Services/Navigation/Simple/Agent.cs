using BEPUutilities;

namespace Lockstep.Framework.Services.Navigation.Simple
{
    class Agent
    {
        public readonly int Id;

        public Vector2 Position { get; set; }
        public Vector2 Destination { get; set; }

        public Agent(int id, Vector2 position)
        {
            Id = id;
            Position = position;
            Destination = position;
        }
    }
}
