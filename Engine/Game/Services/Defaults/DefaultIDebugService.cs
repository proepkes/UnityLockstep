using System;
using System.Collections.Generic;
using BEPUutilities;

namespace Lockstep.Game.Services.Defaults
{
    public class DefaultDebugService : IDebugService
    {
        public Dictionary<uint, Dictionary<uint, Vector2>> Buffer { get; } = new Dictionary<uint, Dictionary<uint, Vector2>>(5000);
        public Dictionary<uint, long> Hashes { get; } = new Dictionary<uint, long>(5000);


        public void Register(uint tick, long hash)
        {
            Hashes.Add(tick, hash);
        }

        public long GetHash(uint tick)
        {
            return Hashes[tick];
        }           

        public bool HasHash(uint tick)
        {
            return Hashes.ContainsKey(tick);
        }

        public void Register(uint tick, uint entityId, Vector2 pos)
        {           
            if (!Buffer.ContainsKey(tick))
            {
                Buffer.Add(tick, new Dictionary<uint, Vector2>(10));
            }

            if (!Buffer[tick].ContainsKey(entityId))
            {
                Buffer[tick].Add(entityId, pos); //Initial size of 5 commands per frame per player
            }
            else
            {
                throw new Exception();
            }
        }

        public bool Validate(uint tick, uint entityId, Vector2 pos)
        {
            return Buffer[tick][entityId].X.RawValue == pos.X.RawValue &&
                   Buffer[tick][entityId].Y.RawValue == pos.Y.RawValue;
        }
    }
}
