using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Assets.Scripts.Game.ComponentData
{
    [Serializable]
    public struct Destination : IComponentData
    {
        public float2 Value;
    }
}