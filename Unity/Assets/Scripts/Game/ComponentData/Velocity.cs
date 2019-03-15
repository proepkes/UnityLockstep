using Unity.Entities;
using Unity.Mathematics;

namespace Assets.Scripts.Game.ComponentData
{
    public struct Velocity : IComponentData
    {          
        public float3 Value;
    }
}