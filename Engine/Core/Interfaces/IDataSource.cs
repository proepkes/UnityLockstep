namespace Lockstep.Core.Interfaces
{
    public interface IDataSource<T> : IService
    {      
        uint Count { get; }

        uint ItemIndex { get; }

        void Insert(T item);

        T GetNext();
    }
}