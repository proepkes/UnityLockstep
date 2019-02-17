using System;
using Lockstep.Core.State.Game.KdTree.Math;

namespace Lockstep.Core.State.Game.KdTree
{
    public interface INearestNeighbourList<TItem, TDistance>
	{
		bool Add(TItem item, TDistance distance);
		TItem GetFurtherest();
		TItem RemoveFurtherest();

		int MaxCapacity { get; }
		int Count { get; }
	}

	public class NearestNeighbourList<TItem, TDistance> : INearestNeighbourList<TItem, TDistance>
	{
		public NearestNeighbourList(int maxCapacity, ITypeMath<TDistance> distanceMath)
		{
			this.MaxCapacity = maxCapacity;
			this.distanceMath = distanceMath;

			queue = new PriorityQueue<TItem, TDistance>(maxCapacity, distanceMath);
		}

		public NearestNeighbourList(ITypeMath<TDistance> distanceMath)
		{
			MaxCapacity = int.MaxValue;
			this.distanceMath = distanceMath;

			queue = new PriorityQueue<TItem, TDistance>(distanceMath);
		}

		private PriorityQueue<TItem, TDistance> queue;

		private ITypeMath<TDistance> distanceMath;

        public int MaxCapacity { get; }

        public int Count => queue.Count;

        public bool Add(TItem item, TDistance distance)
		{
			if (queue.Count >= MaxCapacity)
            {
                // If the distance of this item is less than the distance of the last item
				// in our neighbour list then pop that neighbour off and push this one on
				// otherwise don't even bother adding this item
				if (distanceMath.Compare(distance, queue.GetHighestPriority()) < 0)
				{
					queue.Dequeue();
					queue.Enqueue(item, distance);
					return true;
				}

                return false;
            }

            queue.Enqueue(item, distance);
            return true;
        }

		public TItem GetFurtherest()
        {
            if (Count == 0)
				throw new Exception("List is empty");
            return queue.GetHighest();
        }

		public TDistance GetFurtherestDistance()
        {
            if (Count == 0)
				throw new Exception("List is empty");
            return queue.GetHighestPriority();
        }

		public TItem RemoveFurtherest()
		{
			return queue.Dequeue();
		}

		public bool IsCapacityReached => Count == MaxCapacity;
    }
}
