using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entitas;
using Lockstep.Core.Interfaces;

namespace Lockstep.Core.Systems
{
    public class VerifyShadows : IExecuteSystem
    {
        private readonly Services _services;
        private IGroup<GameEntity> shadows;

        public VerifyShadows(Contexts contexts, Services services)
        {
            _services = services;
            shadows = contexts.game.GetGroup(GameMatcher.Shadow);
        }
        public void Execute()
        {
            var temp = new Dictionary<uint, List<Tuple<byte, uint>>>();
            foreach (var shadow in shadows)
            {
                if (temp.ContainsKey(shadow.tick.value))
                {
                    var others = temp[shadow.tick.value];
                    foreach (var other in others)
                    {
                        if (other.Item1 == shadow.ownerId.value && other.Item2 == shadow.id.value)
                        {
                            _services.Get<ILogService>().Warn("Shadow duplicate!");
                        }
                    }
                }
                else
                {
                    temp.Add(shadow.tick.value, new List<Tuple<byte, uint>>());
                }

                temp[shadow.tick.value].Add(new Tuple<byte, uint>(shadow.ownerId.value, shadow.id.value));
            }
        }
    }
}
