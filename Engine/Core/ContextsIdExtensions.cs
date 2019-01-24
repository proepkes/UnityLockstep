using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entitas;

namespace Lockstep.Core
{
    public static class ContextsIdExtensions
    {
        public static void SubscribeId(this Contexts contexts)
        {         
            contexts.game.OnEntityCreated += (context, entity) => ((GameEntity)entity).AddId(entity.creationIndex);
        }
    }
}
