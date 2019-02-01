using System;
using System.Collections.Generic;   
using Lockstep.Core.Interfaces;

namespace Lockstep.Core.DefaultServices
{
    /// <summary>
    /// Every player has its own id-counter
    /// </summary>
    public class DefaultPlayerEntityIdProvider : IPlayerEntityIdProvider
    {
        private readonly Dictionary<byte, uint> _idMap = new Dictionary<byte, uint>();

        public uint Get(byte key)
        {
            if (!_idMap.ContainsKey(key))
            {
                _idMap.Add(key, 0);
            }
            return _idMap[key];
        }

        public uint GetNext(byte key)
        {
            if (!_idMap.ContainsKey(key))
            {
                _idMap.Add(key, 0);
            }

            var nextId = _idMap[key];
            _idMap[key] += 1;
            return nextId;
        }

        public void SetNext(byte key, uint value)
        {
            if (!_idMap.ContainsKey(key))
            {
                _idMap.Add(key, value);
            }
            else
            {
                _idMap[key] = value;
            }
        }
    }
}
