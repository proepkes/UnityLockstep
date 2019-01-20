namespace ECS.DefaultServices
{
    class DefaultHashService : IHashService
    {
        public long GetHashCode(GameEntity entity)
        {
            long hashCode = 0;
            hashCode ^= entity.position.value.X.RawValue;
            hashCode ^= entity.position.value.Y.RawValue;
            return hashCode;
        }
    }
}
