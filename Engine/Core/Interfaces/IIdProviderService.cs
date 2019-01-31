namespace Lockstep.Core.Interfaces
{
    public interface IIdProviderService<TKey, TValue> : IService
    {
        TValue Get(TKey key);

        TValue GetNext(TKey key);

        void SetNext(TKey key, TValue value);
    }

    public interface IPlayerEntityIdProvider : IIdProviderService<byte, uint>
    {

    }
}
