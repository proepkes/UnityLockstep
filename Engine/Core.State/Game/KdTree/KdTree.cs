using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Lockstep.Core.State.Game.KdTree.Math;

namespace Lockstep.Core.State.Game.KdTree
{
	public enum AddDuplicateBehavior
	{
		Skip,
		Error,
		Update 
	}

	public class DuplicateNodeError : Exception
	{
		public DuplicateNodeError()
			: base("Cannot Add Node With Duplicate Coordinates")
		{
		}
	}

	[Serializable]
	public class KdTree<TKey, TValue> : IKdTree<TKey, TValue>
	{
		public KdTree(int dimensions, ITypeMath<TKey> typeMath)
		{
			_dimensions = dimensions;
			_typeMath = typeMath;
			Count = 0;
		}

		public KdTree(int dimensions, ITypeMath<TKey> typeMath, AddDuplicateBehavior addDuplicateBehavior)
			: this(dimensions, typeMath)
		{
			AddDuplicateBehavior = addDuplicateBehavior;
		}

		private readonly int _dimensions;

		private readonly ITypeMath<TKey> _typeMath;

		private KdTreeNode<TKey, TValue> _root;

        private AddDuplicateBehavior AddDuplicateBehavior { get; }

		public bool Add(TKey[] point, TValue value)
		{
			var nodeToAdd = new KdTreeNode<TKey, TValue>(point, value);

			if (_root == null)
			{
				_root = new KdTreeNode<TKey, TValue>(point, value);
			}
			else
			{
				var dimension = -1;
				var parent = _root;

				do
				{
					// Increment the dimension we're searching in
					dimension = (dimension + 1) % _dimensions;

					// Does the node we're adding have the same hyperpoint as this node?
					if (_typeMath.AreEqual(point, parent.Point))
					{
						switch (AddDuplicateBehavior)
						{
							case AddDuplicateBehavior.Skip:
								return false;

							case AddDuplicateBehavior.Error:
								throw new DuplicateNodeError();

							case AddDuplicateBehavior.Update:
								parent.Value = value;
								return true;   

                            default:
								// Should never happen
								throw new Exception("Unexpected AddDuplicateBehavior");
						}
					}

					// Which side does this node sit under in relation to it's parent at this level?
					var compare = _typeMath.Compare(point[dimension], parent.Point[dimension]);

					if (parent[compare] == null)
					{
						parent[compare] = nodeToAdd;
						break;
					}

                    parent = parent[compare];
                }
				while (true);
			}

			Count++;
			return true;
		}

		private void ReadChildNodes(KdTreeNode<TKey, TValue> removedNode)
		{
			if (removedNode.IsLeaf)
				return;

			// The following code might seem a little redundant but we're using 
			// 2 queues so we can add the child nodes back in, in (more or less) 
			// the same order they were added in the first place
			var nodesToRead = new Queue<KdTreeNode<TKey, TValue>>();

			var nodesToReadQueue = new Queue<KdTreeNode<TKey, TValue>>();

			if (removedNode.LeftChild != null)
				nodesToReadQueue.Enqueue(removedNode.LeftChild);

			if (removedNode.RightChild != null)
				nodesToReadQueue.Enqueue(removedNode.RightChild);

			while (nodesToReadQueue.Count > 0)
			{
				var nodeToRead = nodesToReadQueue.Dequeue();

				nodesToRead.Enqueue(nodeToRead);

				for (var side = -1; side <= 1; side += 2)
				{
					if (nodeToRead[side] != null)
					{
						nodesToReadQueue.Enqueue(nodeToRead[side]);

						nodeToRead[side] = null;
					}
				}
			}

			while (nodesToRead.Count > 0)
			{
				var nodeToRead = nodesToRead.Dequeue();

				Count--;
				Add(nodeToRead.Point, nodeToRead.Value);
			}
		}

		public void RemoveAt(TKey[] point)
		{
			// Is tree empty?
			if (_root == null)
				return;

			KdTreeNode<TKey, TValue> node;

			if (_typeMath.AreEqual(point, _root.Point))
			{
				node = _root;
				_root = null;
				Count--;
				ReadChildNodes(node);
				return;
			}

			node = _root;

			var dimension = -1;
			do
			{
				dimension = (dimension + 1) % _dimensions;

				var compare = _typeMath.Compare(point[dimension], node.Point[dimension]);

				if (node[compare] == null)
					// Can't find node
					return;

				if (_typeMath.AreEqual(point, node[compare].Point))
				{
					var nodeToRemove = node[compare];
					node[compare] = null;
					Count--;

					ReadChildNodes(nodeToRemove);
				}
				else
					node = node[compare];
			}
			while (node != null);
		}

		public KdTreeNode<TKey, TValue>[] GetNearestNeighbors(TKey[] point, int count)
		{
			if (count > Count)
				count = Count;

			if (count < 0)
			{
				throw new ArgumentException("Number of neighbors cannot be negative");
			}

			if (count == 0)
				return new KdTreeNode<TKey, TValue>[0];

            var nearestNeighbors = new NearestNeighbourList<KdTreeNode<TKey, TValue>, TKey>(count, _typeMath);

			var rect = HyperRect<TKey>.Infinite(_dimensions, _typeMath);

			AddNearestNeighbors(_root, point, rect, 0, nearestNeighbors, _typeMath.MaxValue);

			count = nearestNeighbors.Count;

			var neighborArray = new KdTreeNode<TKey, TValue>[count];

			for (var index = 0; index < count; index++)
				neighborArray[count - index - 1] = nearestNeighbors.RemoveFurtherest();

			return neighborArray;
		}

        /*
		 * 1. Search for the target
		 * 
		 *   1.1 Start by splitting the specified hyper rect
		 *       on the specified node's point along the current
		 *       dimension so that we end up with 2 sub hyper rects
		 *       (current dimension = depth % dimensions)
		 *   
		 *	 1.2 Check what sub rectangle the the target point resides in
		 *	     under the current dimension
		 *	     
		 *   1.3 Set that rect to the nearer rect and also the corresponding 
		 *       child node to the nearest rect and node and the other rect 
		 *       and child node to the further rect and child node (for use later)
		 *       
		 *   1.4 Travel into the nearer rect and node by calling function
		 *       recursively with nearer rect and node and incrementing 
		 *       the depth
		 * 
		 * 2. Add leaf to list of nearest neighbors
		 * 
		 * 3. Walk back up tree and at each level:
		 * 
		 *    3.1 Add node to nearest neighbors if
		 *        we haven't filled our nearest neighbor
		 *        list yet or if it has a distance to target less
		 *        than any of the distances in our current nearest 
		 *        neighbors.
		 *        
		 *    3.2 If there is any point in the further rectangle that is closer to
		 *        the target than our furthest nearest neighbor then travel into
		 *        that rect and node
		 * 
		 *  That's it, when it finally finishes traversing the branches 
		 *  it needs to we'll have our list!
		 */

        private void AddNearestNeighbors(
			KdTreeNode<TKey, TValue> node,
			TKey[] target,
			HyperRect<TKey> rect,
			int depth,
			NearestNeighbourList<KdTreeNode<TKey, TValue>, TKey> nearestNeighbours,
			TKey maxSearchRadiusSquared)
		{
			if (node == null)
				return;

			// Work out the current dimension
			var dimension = depth % _dimensions;

			// Split our hyper-rect into 2 sub rects along the current 
			// node's point on the current dimension
			var leftRect = rect.Clone();
			leftRect.MaxPoint[dimension] = node.Point[dimension];

			var rightRect = rect.Clone();
			rightRect.MinPoint[dimension] = node.Point[dimension];

			// Which side does the target reside in?
			var compare = _typeMath.Compare(target[dimension], node.Point[dimension]);

			var nearerRect = compare <= 0 ? leftRect : rightRect;
			var furtherRect = compare <= 0 ? rightRect : leftRect;

			var nearerNode = compare <= 0 ? node.LeftChild : node.RightChild;
			var furtherNode = compare <= 0 ? node.RightChild : node.LeftChild;

			// Let's walk down into the nearer branch
			if (nearerNode != null)
			{
				AddNearestNeighbors(
					nearerNode,
					target,
					nearerRect,
					depth + 1,
					nearestNeighbours,
					maxSearchRadiusSquared);
			}

			TKey distanceSquaredToTarget;

			// Walk down into the further branch but only if our capacity hasn't been reached 
			// OR if there's a region in the further rect that's closer to the target than our
			// current furthest nearest neighbor
			var closestPointInFurtherRect = furtherRect.GetClosestPoint(target, _typeMath);
			distanceSquaredToTarget = _typeMath.DistanceSquaredBetweenPoints(closestPointInFurtherRect, target);

			if (_typeMath.Compare(distanceSquaredToTarget, maxSearchRadiusSquared) <= 0)
			{
				if (nearestNeighbours.IsCapacityReached)
				{
					if (_typeMath.Compare(distanceSquaredToTarget, nearestNeighbours.GetFurtherestDistance()) < 0)
						AddNearestNeighbors(
							furtherNode,
							target,
							furtherRect,
							depth + 1,
							nearestNeighbours,
							maxSearchRadiusSquared);
				}
				else
				{
					AddNearestNeighbors(
						furtherNode,
						target,
						furtherRect,
						depth + 1,
						nearestNeighbours,
						maxSearchRadiusSquared);
				}
			}

			// Try to add the current node to our nearest neighbors list
			distanceSquaredToTarget = _typeMath.DistanceSquaredBetweenPoints(node.Point, target);

			if (_typeMath.Compare(distanceSquaredToTarget, maxSearchRadiusSquared) <= 0)
				nearestNeighbours.Add(node, distanceSquaredToTarget);
		}

		/// <summary>
		/// Performs a radial search.
		/// </summary>
		/// <param name="center">Center point</param>
		/// <param name="radius">Radius to find neighbours within</param>
		public KdTreeNode<TKey, TValue>[] RadialSearch(TKey[] center, TKey radius)
		{
			return RadialSearch(center, radius, new NearestNeighbourList<KdTreeNode<TKey, TValue>, TKey>(_typeMath));
		}

		/// <summary>
		/// Performs a radial search up to a maximum count.
		/// </summary>
		/// <param name="center">Center point</param>
		/// <param name="radius">Radius to find neighbours within</param>
		/// <param name="count">Maximum number of neighbours</param>
		public KdTreeNode<TKey, TValue>[] RadialSearch(TKey[] center, TKey radius, int count)
		{
			return RadialSearch(center, radius, new NearestNeighbourList<KdTreeNode<TKey, TValue>, TKey>(count, _typeMath));
		}

		private KdTreeNode<TKey, TValue>[] RadialSearch(TKey[] center, TKey radius, NearestNeighbourList<KdTreeNode<TKey, TValue>, TKey> nearestNeighbors)
		{
			AddNearestNeighbors(
				_root,
				center,
				HyperRect<TKey>.Infinite(_dimensions, _typeMath),
				0,
				nearestNeighbors,
				radius);

			var count = nearestNeighbors.Count;

			var neighborArray = new KdTreeNode<TKey, TValue>[count];

			for (var index = 0; index < count; index++)
				neighborArray[count - index - 1] = nearestNeighbors.RemoveFurtherest();

			return neighborArray;
		}

		public int Count { get; private set; }

		public bool TryFindValueAt(TKey[] point, out TValue value)
		{
			var parent = _root;
			var dimension = -1;
			do
			{
				if (parent == null)
				{
					value = default;
					return false;
				}

                if (_typeMath.AreEqual(point, parent.Point))
                {
                    value = parent.Value;
                    return true;
                }

                // Keep searching
				dimension = (dimension + 1) % _dimensions;
				var compare = _typeMath.Compare(point[dimension], parent.Point[dimension]);
				parent = parent[compare];
			}
			while (true);
		}

		public TValue FindValueAt(TKey[] point)
        {
            if (TryFindValueAt(point, out var value))
				return value;
            return default;
        }

		public bool TryFindValue(TValue value, out TKey[] point)
		{
			if (_root == null)
			{
				point = null;
				return false;
			}

			// First-in, First-out list of nodes to search
			var nodesToSearch = new Queue<KdTreeNode<TKey, TValue>>();

			nodesToSearch.Enqueue(_root);

			while (nodesToSearch.Count > 0)
			{
				var nodeToSearch = nodesToSearch.Dequeue();

				if (nodeToSearch.Value.Equals(value))
				{
					point = nodeToSearch.Point;
					return true;
				}

                for (var side = -1; side <= 1; side += 2)
                {
                    var childNode = nodeToSearch[side];

                    if (childNode != null)
                        nodesToSearch.Enqueue(childNode);
                }
            }

			point = null;
			return false;
		}

		public TKey[] FindValue(TValue value)
        {
            if (TryFindValue(value, out var point))
				return point;
            return null;
        }

		private void AddNodeToStringBuilder(KdTreeNode<TKey, TValue> node, StringBuilder sb, int depth)
		{
			sb.AppendLine(node.ToString());

			for (var side = -1; side <= 1; side += 2)
			{
				for (var index = 0; index <= depth; index++)
					sb.Append("\t");

				sb.Append(side == -1 ? "L " : "R ");

				if (node[side] == null)
					sb.AppendLine("");
				else
					AddNodeToStringBuilder(node[side], sb, depth + 1);
			}
		}

		public override string ToString()
		{
			if (_root == null)
				return "";

			var sb = new StringBuilder();
			AddNodeToStringBuilder(_root, sb, 0);
			return sb.ToString();
		}

		private void AddNodesToList(KdTreeNode<TKey, TValue> node, List<KdTreeNode<TKey, TValue>> nodes)
		{
			if (node == null)
				return;

			nodes.Add(node);

			for (var side = -1; side <= 1; side += 2)
			{
				if (node[side] != null)
				{
					AddNodesToList(node[side], nodes);
					node[side] = null;
				}
			}
		}

		private void SortNodesArray(KdTreeNode<TKey, TValue>[] nodes, int byDimension, int fromIndex, int toIndex)
		{
			for (var index = fromIndex + 1; index <= toIndex; index++)
			{
				var newIndex = index;

				while (true)
				{
					var a = nodes[newIndex - 1];
					var b = nodes[newIndex];
					if (_typeMath.Compare(b.Point[byDimension], a.Point[byDimension]) < 0)
					{
						nodes[newIndex - 1] = b;
						nodes[newIndex] = a;
					}
					else
						break;
				}
			}
		}

		private void AddNodesBalanced(KdTreeNode<TKey, TValue>[] nodes, int byDimension, int fromIndex, int toIndex)
		{
			if (fromIndex == toIndex)
			{
				Add(nodes[fromIndex].Point, nodes[fromIndex].Value);
				nodes[fromIndex] = null;
				return;
			}

			// Sort the array from the fromIndex to the toIndex
			SortNodesArray(nodes, byDimension, fromIndex, toIndex);

			// Find the splitting point
			var midIndex = fromIndex + (int)System.Math.Round((toIndex + 1 - fromIndex) / 2f) - 1;

			// Add the splitting point
			Add(nodes[midIndex].Point, nodes[midIndex].Value);
			nodes[midIndex] = null;

			// Recurse
			var nextDimension = (byDimension + 1) % _dimensions;

			if (fromIndex < midIndex)
				AddNodesBalanced(nodes, nextDimension, fromIndex, midIndex - 1);

			if (toIndex > midIndex)
				AddNodesBalanced(nodes, nextDimension, midIndex + 1, toIndex);
		}

		public void Balance()
		{
			var nodeList = new List<KdTreeNode<TKey, TValue>>();
			AddNodesToList(_root, nodeList);

			Clear();

			AddNodesBalanced(nodeList.ToArray(), 0, 0, nodeList.Count - 1);
		}

		private void RemoveChildNodes(KdTreeNode<TKey, TValue> node)
		{
			for (var side = -1; side <= 1; side += 2)
			{
				if (node[side] != null)
				{
					RemoveChildNodes(node[side]);
					node[side] = null;
				}
			}
		}

		public void Clear()
		{
			if (_root != null)
				RemoveChildNodes(_root);
		}

		public void SaveToFile(string filename)
		{
			var formatter = new BinaryFormatter();
			using (var stream = File.Create(filename))
			{
				formatter.Serialize(stream, this);
				stream.Flush();
			}
		}

		public static KdTree<TKey, TValue> LoadFromFile(string filename)
		{
			var formatter = new BinaryFormatter();
			using (var stream = File.Open(filename, FileMode.Open))
			{
				return (KdTree<TKey, TValue>)formatter.Deserialize(stream);
			}

		}

		public IEnumerator<KdTreeNode<TKey, TValue>> GetEnumerator()
		{
			var left = new Stack<KdTreeNode<TKey, TValue>>();
			var right = new Stack<KdTreeNode<TKey, TValue>>();

			void AddLeft(KdTreeNode<TKey, TValue> node)
			{
				if (node.LeftChild != null)
				{
					left.Push(node.LeftChild);
				}
			}

			void AddRight(KdTreeNode<TKey, TValue> node)
			{
				if (node.RightChild != null)
				{
					right.Push(node.RightChild);
				}
			}

			if (_root != null)
			{
				yield return _root;

				AddLeft(_root);
				AddRight(_root);

				while (true)
				{
					if (left.Any())
					{
						var item = left.Pop();

						AddLeft(item);
						AddRight(item);

						yield return item;
					}
					else if (right.Any())
					{
						var item = right.Pop();

						AddLeft(item);
						AddRight(item);

						yield return item;
					}
					else
					{
						break;
					}
				}
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}