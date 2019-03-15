using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.Game.ComponentData
{
    [RequiresEntityConversion]
    public class DestinationProxy : MonoBehaviour, IConvertGameObjectToEntity
    {                         
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new Destination());
        }
    }
}