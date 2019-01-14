using System.Collections.Generic;
using System.Linq;
using BEPUphysics.CollisionShapes.ConvexShapes;
using BEPUphysics.Entities;
using BEPUutilities;
using FixMath.NET;                  
using Lockstep.Framework.Pathfinding;

namespace Lockstep.Framework
{                   
    public class LockstepAgent : ILockstepAgent
    {
        public ulong ID { get; set; }

        public Entity Body { get; }

        public Fix64 Radius { get; }                                                             
                                        
		//TODO: Put all this stuff in an extendible class
		public LSInfluencer Influencer { get; private set; } 

        public byte ControllerID = 0;      

        private ICollection<ILockstepAbility> Abilities { get; } = new List<ILockstepAbility>();

        public LockstepAgent(Fix64 radius, Vector2 position = default(Vector2), Vector2 rotation = default(Vector2))
        {      
            Radius = radius;                           

            if (Influencer.IsNotNull())
            {
                Influencer.Initialize();
            }


            Influencer = new LSInfluencer();       
            Influencer.Setup(this);

            Body = new Entity(new BoxShape(1, 1, 1));                     
        }

        public void AddAbility(ILockstepAbility ability)
        {
            ability.Setup(this);
            Abilities.Add(ability);
        }

        public T GetAbility<T>() where T : ILockstepAbility
        {
            return (T) Abilities.FirstOrDefault(ability => ability.GetType() == typeof(T));
        } 

        public void Simulate()
		{     
			if (Influencer.IsNotNull())
			{
				Influencer.Simulate();
			}

            foreach (var ability in Abilities)
            {
                ability.Simulate();
            }
		}                          

		public ulong GetHashCode()
		{
			ulong hash = 3;          
			hash ^= this.ID;
			hash ^= this.Body.Position.GetLongHashCode();    
			hash ^= this.Body.LinearVelocity.GetLongHashCode();
			return hash;
		}     
	}
}