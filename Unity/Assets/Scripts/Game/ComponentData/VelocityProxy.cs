using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.Scripts.Game.ComponentData
{
    [RequiresEntityConversion]
    public class VelocityProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        public float3 Value;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new Velocity
            {
                Value = Value
            });
        }
    }
}