using System.Collections.Generic;
using Entitas;
using Lockstep.Game.Interfaces;

namespace Lockstep.Game.Features.Cleanup
{
    public class RemoveDestroyedEntitiesFromView : ICleanupSystem
    {
        private readonly IGroup<GameEntity> _group;
        private readonly List<GameEntity> _buffer = new List<GameEntity>();

        private readonly IViewService _viewService;              

        public RemoveDestroyedEntitiesFromView(Contexts contexts, ServiceContainer services)
        {
            _group = contexts.game.GetGroup(GameMatcher.Destroyed);

            _viewService = services.Get<IViewService>();               
        }

        public void Cleanup()
        {
            foreach (var e in _group.GetEntities(_buffer))
            {
                _viewService.DeleteView(e.localId.value);        
            }
        }
    }
}