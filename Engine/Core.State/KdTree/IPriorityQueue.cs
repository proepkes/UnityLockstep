namespace Lockstep.Core.State.KdTree
{
    public interface IPriorityQueue<TItem, in TPriority>
	{
		void Enqueue(TItem item, TPriority priority);

		TItem Dequeue();

		int Count { get; }
	}
}
