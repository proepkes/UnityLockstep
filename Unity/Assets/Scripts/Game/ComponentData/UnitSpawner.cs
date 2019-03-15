using Unity.Entities;

namespace Assets.Scripts.Game.ComponentData
{
    public struct UnitSpawner : IComponentData
    {
        public int CountX;
        public int CountY;
        public Entity Prefab;
    }
}
