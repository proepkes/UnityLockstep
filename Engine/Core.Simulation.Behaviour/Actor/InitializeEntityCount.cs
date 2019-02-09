using System;
using System.Collections.Generic;
using System.Text;
using Entitas;

namespace Lockstep.Core.Systems.Actor
{
    public class InitializeEntityCount : IInitializeSystem
    {
        private ActorContext _actorContext;

        public InitializeEntityCount(Contexts contexts)
        {
            _actorContext = contexts.actor;
        }
        public void Initialize()
        {
            foreach (var actor in _actorContext.GetEntities())
            {
                actor.AddEntityCount(0);
            }
        }
    }
}
