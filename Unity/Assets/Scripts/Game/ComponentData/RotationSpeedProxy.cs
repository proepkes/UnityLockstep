using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.Scripts.Game.ComponentData
{
    [RequiresEntityConversion]
    public class RotationSpeedProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        public float DegreesPerSecond = 360;
        
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var data = new RotationSpeed { DegreesPerSecond = DegreesPerSecond };
            dstManager.AddComponentData(entity, data);
        }
    }
}
