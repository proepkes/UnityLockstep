using System;
using System.Collections.Generic;
using Entitas;
using Lockstep.Core.Interfaces;

namespace Lockstep.Core.Systems.Debugging
{
    public class VerifyNoDuplicateBackups : IExecuteSystem
    {
        private readonly Services _services;
        private readonly IGroup<GameEntity> _backups;

        public VerifyNoDuplicateBackups(Contexts contexts, Services services)
        {
            _services = services;
            _backups = contexts.game.GetGroup(GameMatcher.Backup);
        }
        public void Execute()
        {
            var temp = new Dictionary<uint, List<uint>>();
            foreach (var entity in _backups)
            {
                if (temp.ContainsKey(entity.backup.tick))
                {         
                    if (temp[entity.backup.tick].Contains(entity.backup.localEntityId))
                    {     
                        _services.Get<ILogService>().Warn("Shadow duplicate!");
                    }    
                }
                else
                {
                    temp.Add(entity.backup.tick, new List<uint>());
                }

                temp[entity.backup.tick].Add(entity.backup.localEntityId);
            }
        }
    }
}
