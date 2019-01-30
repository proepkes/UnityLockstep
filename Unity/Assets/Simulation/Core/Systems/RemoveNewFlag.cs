using System.Collections.Generic;   
using Entitas;

namespace Lockstep.Core.Systems
{     
    public sealed class RemoveNewFlag : ICleanupSystem
    {
        private readonly IGroup<GameEntity> _group;
        private readonly List<GameEntity> _buffer = new List<GameEntity>();

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
