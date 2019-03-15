using System;
using Unity.Entities;

namespace Assets.Scripts.Game.ComponentData
{
    [Serializable]
    public struct MovementSpeed : IComponentData
    {
        public float Value;
    }
}