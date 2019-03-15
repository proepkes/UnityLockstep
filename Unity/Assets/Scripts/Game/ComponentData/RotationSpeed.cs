using System;
using Unity.Entities;

namespace Assets.Scripts.Game.ComponentData
{
    // Serializable attribute is for editor support.
    [Serializable]
    public struct RotationSpeed : IComponentData
    {
        public float DegreesPerSecond;
    }
}

