using System;
using System.Collections.Generic;
using Entitas;
using Lockstep.Core.Interfaces;

namespace Lockstep.Core.Systems.Debugging
{
    public class VerifyNoDuplicateShadows : IExecuteSystem
    {
        private readonly Services _services;
        private readonly IGroup<GameEntity> _shadows;

        public VerifyNoDuplicateShadows(Contexts contexts, Services services)
        {
            _services = services;
            _shadows = contexts.game.GetGroup(GameMatcher.Shadow);
        }
        public void Execute()
        {
            var temp = new Dictionary<uint, List<Tuple<byte, uint>>>();
            foreach (var shadow in _shadows)
            {
                if (temp.ContainsKey(shadow.tick.value))
                {
                    var others = temp[shadow.tick.value];
                    foreach (var other in others)
                    {
                        if (other.Item1 == shadow.actorId.value && other.Item2 == shadow.id.value)
                        {
                            _services.Get<ILogService>().Warn("Shadow duplicate!");
                        }
                    }
                }
                else
                {
                    temp.Add(shadow.tick.value, new List<Tuple<byte, uint>>());
                }

                temp[shadow.tick.value].Add(new Tuple<byte, uint>(shadow.actorId.value, shadow.id.value));
            }
        }
    }
}
