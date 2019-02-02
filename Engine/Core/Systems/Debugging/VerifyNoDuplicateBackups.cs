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
            var temp = new Dictionary<uint, List<Tuple<byte, uint>>>();
            foreach (var entity in _backups)
            {
                if (temp.ContainsKey(entity.backup.tick))
                {
                    var others = temp[entity.backup.tick];
                    foreach (var other in others)
                    {
                        if (other.Item1 == entity.actorId.value && other.Item2 == entity.id.value)
                        {
                            _services.Get<ILogService>().Warn("Shadow duplicate!");
                        }
                    }
                }
                else
                {
                    temp.Add(entity.backup.tick, new List<Tuple<byte, uint>>());
                }

                temp[entity.backup.tick].Add(new Tuple<byte, uint>(entity.actorId.value, entity.id.value));
            }
        }
    }
}
