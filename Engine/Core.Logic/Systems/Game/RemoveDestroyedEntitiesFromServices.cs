using System;
using System.Collections.Generic;
using System.Text;
using Entitas;
using Lockstep.Core.Logic.Interfaces.Services;

namespace Lockstep.Core.Logic.Systems.Game
{
    public class RemoveDestroyedEntitiesFromServices : ICleanupSystem
    {
        private readonly IGroup<GameEntity> _group;
        private readonly List<GameEntity> _buffer = new List<GameEntity>();

        private readonly IViewService _viewService;
        private readonly INavigationService _navigationService;

        public RemoveDestroyedEntitiesFromServices(Contexts contexts, ServiceContainer services)
        {
            _group = contexts.game.GetGroup(GameMatcher.Destroyed);

            _viewService = services.Get<IViewService>();
            _navigationService = services.Get<INavigationService>();
        }

        public void Cleanup()
        {
            foreach (var e in _group.GetEntities(_buffer))
            {
                _viewService.DeleteView(e.localId.value);
                _navigationService.RemoveAgent(e.localId.value);
            }
        }
    }
}