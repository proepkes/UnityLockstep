using System.Collections.Generic;   
using Entitas;

namespace Lockstep.Core.Systems
{     
    public sealed class RemoveNewFlag : ICleanupSystem
    {              
        readonly IGroup<GameEntity> _group;
        readonly List<GameEntity> _buffer = new List<GameEntity>();

        public RemoveNewFlag(Contexts contexts)
        {
            _group = contexts.game.GetGroup(GameMatcher.AllOf(GameMatcher.Id, GameMatcher.New));
        }

        public void Cleanup()
        {
            foreach (var e in _group.GetEntities(_buffer))
            {
                e.isNew = false;
            }
        }
    }
}
