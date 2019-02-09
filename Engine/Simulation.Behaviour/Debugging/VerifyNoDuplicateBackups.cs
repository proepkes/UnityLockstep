using System.Collections.Generic;
using Entitas;
using Lockstep.Core.Services;
using Simulation.Behaviour.Services;

namespace Simulation.Behaviour.Debugging
{
    public class VerifyNoDuplicateBackups : IExecuteSystem
    {
        private readonly ServiceContainer serviceContainer;
        private readonly IGroup<GameEntity> _backups;

        public VerifyNoDuplicateBackups(Contexts contexts, ServiceContainer serviceContainer)
        {
            this.serviceContainer = serviceContainer;
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
                        serviceContainer.Get<ILogService>().Warn(() => "Backup duplicate: " + temp[entity.backup.tick].Count + " backups in tick "+ entity.backup.tick +" are already pointing to " + entity.backup.localEntityId);
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
