// <copyright file="KDTree.cs" company="Eric Regina">
// Copyright (c) Eric Regina. All rights reserved.
// </copyright>

using FixMath.NET;

namespace Supercluster.KDTree
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Supercluster.KDTree.Utilities;
    using static Utilities.BinaryTreeNavigation;

    /// <summary>
    /// Represents a KD-Tree. KD-Trees are used for fast spatial searches. Searching in a
    /// balanced KD-Tree is O(log n) where linear search is O(n). Points in the KD-Tree are
    /// equi-length arrays of type <typeparamref name="TDimension"/>. The node objects associated
    /// with the points is an array of type <typeparamref name="TNode"/>.
    /// </summary>
    /// <remarks>
    /// KDTrees can be fairly difficult to understand at first. The following references helped me
    /// understand what exactly a KDTree is doing and the contain the best descriptions of searches in a KDTree.
    /// Samet's book is the best reference of multidimensional data structures I have ever seen. Wikipedia is also a good starting place.
    /// References:
    /// <ul style="list-style-type:none">
    /// <li> <a href="http://store.elsevier.com/product.jsp?isbn=9780123694461">Foundations of Multidimensional and Metric Data Structures, 1st Edition, by Hanan Samet. ISBN: 9780123694461</a> </li>
    /// <li> <a href="https://en.wikipedia.org/wiki/K-d_tree"> https://en.wikipedia.org/wiki/K-d_tree</a> </li>
    /// </ul>
    /// </remarks>
    /// <typeparam name="TDimension">The type of the dimension.</typeparam>
    /// <typeparam name="TNode">The type representing the actual node objects.</typeparam>
    [Serializable]
    public class KDTree<TDimension, TNode>
        where TDimension : IComparable<TDimension>
    {
        /// <summary>
        /// The number of points in the KDTree
        /// </summary>
        public int Count { get; }

        /// <summary>
        /// The numbers of dimensions that the tree has.
        /// </summary>
        public int Dimensions { get; }

        /// <summary>
        /// The array in which the binary tree is stored. Enumerating this array is a level-order traversal of the tree.
        /// </summary>
        public TDimension[][] InternalPointArray { get; }

        /// <summary>
        /// The array in which the node objects are stored. There is a one-to-one correspondence with this array and the <see cref="InternalPointArray"/>.
        /// </summary>
        public TNode[] InternalNodeArray { get; }

        /// <summary>
        /// The metric function used to calculate distance between points.
        /// </summary>
        public Func<TDimension[], TDimension[], Fix64> Metric { get; set; }

        /// <summary>
        /// Gets a <see cref="BinaryTreeNavigator{TPoint,TNode}"/> that allows for manual tree navigation,
        /// </summary>
        public BinaryTreeNavigator<TDimension[], TNode> Navigator
            => new BinaryTreeNavigator<TDimension[], TNode>(this.InternalPointArray, this.InternalNodeArray);

        /// <summary>
        /// The maximum value along any dimension.
        /// </summary>
        private TDimension MaxValue { get; }

        /// <summary>
        /// The minimum value along any dimension.
        /// </summary>
        private TDimension MinValue { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="KDTree{TDimension,TNode}"/> class.
        /// </summary>
        /// <param name="dimensions">The number of dimensions in the data set.</param>
        /// <param name="points">The points to be constructed into a <see cref="KDTree{TDimension,TNode}"/></param>
        /// <param name="nodes">The nodes associated with each point.</param>
        /// <param name="metric">The metric function which implicitly defines the metric space in which the KDTree operates in. This should satisfy the triangle inequality.</param>
        /// <param name="searchWindowMinValue">The minimum value to be used in node searches. If null, we assume that <typeparamref name="TDimension"/> has a static field named "MinValue". All numeric structs have this field.</param>
        /// <param name="searchWindowMaxValue">The maximum value to be used in node searches. If null, we assume that <typeparamref name="TDimension"/> has a static field named "MaxValue". All numeric structs have this field.</param>
        public KDTree(
            int dimensions,
            TDimension[][] points,
            TNode[] nodes,
            Func<TDimension[], TDimension[], Fix64> metric,
            TDimension searchWindowMinValue = default(TDimension),
            TDimension searchWindowMaxValue = default(TDimension))
        {
            // Attempt find the Min/Max value if null.
            if (searchWindowMinValue.Equals(default(TDimension)))
            {
                var type = typeof(TDimension);
                this.MinValue = (TDimension)type.GetField("MinValue").GetValue(type);
            }
            else
            {
                this.MinValue = searchWindowMinValue;
            }

            if (searchWindowMaxValue.Equals(default(TDimension)))
            {
                var type = typeof(TDimension);
                this.MaxValue = (TDimension)type.GetField("MaxValue").GetValue(type);
            }
            else
            {
                this.MaxValue = searchWindowMaxValue;
            }

            // Calculate the number of nodes needed to contain the binary tree.
            // This is equivalent to finding the power of 2 greater than the number of points
            var elementCount = (int)Math.Pow(2, (int)(Math.Log(points.Length) / Math.Log(2)) + 1);
            this.Dimensions = dimensions;
            this.InternalPointArray = Enumerable.Repeat(default(TDimension[]), elementCount).ToArray();
            this.InternalNodeArray = Enumerable.Repeat(default(TNode), elementCount).ToArray();
            this.Metric = metric;
            this.Count = points.Length;
            this.GenerateTree(0, 0, points, nodes);
        }

        /// <summary>
        /// Finds the nearest neighbors in the <see cref="KDTree{TDimension,TNode}"/> of the given <paramref name="point"/>.
        /// </summary>
        /// <param name="point">The point whose neighbors we search for.</param>
        /// <param name="neighbors">The number of neighbors to look for.</param>
        /// <returns>The</returns>
        public void NearestNeighbors(TDimension[] point, BoundedPriorityList<int, Fix64> result)
        {
            var rect = HyperRect<TDimension>.Infinite(this.Dimensions, this.MaxValue, this.MinValue);
            this.SearchForNearestNeighbors(0, point, rect, 0, result, Fix64.MaxValue);
        }

        /// <summary>
        /// Searches for the closest points in a hyper-sphere around the given center.
        /// </summary>
        /// <param name="center">The center of the hyper-sphere</param>
        /// <param name="radius">The radius of the hyper-sphere</param>
        /// <returns>The specified number of closest points in the hyper-sphere</returns>
        public void RadialSearch(TDimension[] center, Fix64 radius, BoundedPriorityList<int, Fix64> nearestNeighbors)
        {
            this.SearchForNearestNeighbors(
                0,
                center,
                HyperRect<TDimension>.Infinite(this.Dimensions, this.MaxValue, this.MinValue),
                0,
                nearestNeighbors,
                radius);
        }

        /// <summary>
        /// Grows a KD tree recursively via median splitting. We find the median by doing a full sort.
        /// </summary>
        /// <param name="index">The array index for the current node.</param>
        /// <param name="dim">The current splitting dimension.</param>
        /// <param name="points">The set of points remaining to be added to the kd-tree</param>
        /// <param name="nodes">The set of nodes RE</param>
        private void GenerateTree(
            int index,
            int dim,
            IReadOnlyCollection<TDimension[]> points,
            IEnumerable<TNode> nodes)
        {
            // See wikipedia for a good explanation kd-tree construction.
            // https://en.wikipedia.org/wiki/K-d_tree

            // zip both lists so we can sort nodes according to points
            var zippedList = points.Zip(nodes, (p, n) => new { Point = p, Node = n });

            // sort the points along the current dimension
            var sortedPoints = zippedList.OrderBy(z => z.Point[dim]).ToArray();

            // get the point which has the median value of the current dimension.
            var medianPoint = sortedPoints[points.Count / 2];
            var medianPointIdx = sortedPoints.Length / 2;

            // The point with the median value all the current dimension now becomes the value of the current tree node
            // The previous node becomes the parents of the current node.
            this.InternalPointArray[index] = medianPoint.Point;
            this.InternalNodeArray[index] = medianPoint.Node;

            // We now split the sorted points into 2 groups
            // 1st group: points before the median
            var leftPoints = new TDimension[medianPointIdx][];
            var leftNodes = new TNode[medianPointIdx];
            Array.Copy(sortedPoints.Select(z => z.Point).ToArray(), leftPoints, leftPoints.Length);
            Array.Copy(sortedPoints.Select(z => z.Node).ToArray(), leftNodes, leftNodes.Length);

            // 2nd group: Points after the median
            var rightPoints = new TDimension[sortedPoints.Length - (medianPointIdx + 1)][];
            var rightNodes = new TNode[sortedPoints.Length - (medianPointIdx + 1)];
            Array.Copy(
                sortedPoints.Select(z => z.Point).ToArray(),
                medianPointIdx + 1,
                rightPoints,
                0,
                rightPoints.Length);
            Array.Copy(sortedPoints.Select(z => z.Node).ToArray(), medianPointIdx + 1, rightNodes, 0, rightNodes.Length);

            // We new recurse, passing the left and right arrays for arguments.
            // The current node's left and right values become the "roots" for
            // each recursion call. We also forward cycle to the next dimension.
            var nextDim = (dim + 1) % this.Dimensions; // select next dimension

            // We only need to recurse if the point array contains more than one point
            // If the array has no points then the node stay a null value
            if (leftPoints.Length <= 1)
            {
                if (leftPoints.Length == 1)
                {
                    this.InternalPointArray[LeftChildIndex(index)] = leftPoints[0];
                    this.InternalNodeArray[LeftChildIndex(index)] = leftNodes[0];
                }
            }
            else
            {
                this.GenerateTree(LeftChildIndex(index), nextDim, leftPoints, leftNodes);
            }

            // Do the same for the right points
            if (rightPoints.Length <= 1)
            {
                if (rightPoints.Length == 1)
                {
                    this.InternalPointArray[RightChildIndex(index)] = rightPoints[0];
                    this.InternalNodeArray[RightChildIndex(index)] = rightNodes[0];
                }
            }
            else
            {
                this.GenerateTree(RightChildIndex(index), nextDim, rightPoints, rightNodes);
            }
        }

        /// <summary>
        /// A top-down recursive method to find the nearest neighbors of a given point.
        /// </summary>
        /// <param name="nodeIndex">The index of the node for the current recursion branch.</param>
        /// <param name="target">The point whose neighbors we are trying to find.</param>
        /// <param name="rect">The <see cref="HyperRect{T}"/> containing the possible nearest neighbors.</param>
        /// <param name="dimension">The current splitting dimension for this recursion branch.</param>
        /// <param name="nearestNeighbors">The <see cref="BoundedPriorityList{TElement,TPriority}"/> containing the nearest neighbors already discovered.</param>
        /// <param name="maxSearchRadiusSquared">The squared radius of the current largest distance to search from the <paramref name="target"/></param>
        private void SearchForNearestNeighbors(
            int nodeIndex,
            TDimension[] target,
            HyperRect<TDimension> rect,
            int dimension,
            BoundedPriorityList<int, Fix64> nearestNeighbors,
            Fix64 maxSearchRadiusSquared)
        {
            if (this.InternalPointArray.Length <= nodeIndex || nodeIndex < 0
                || this.InternalPointArray[nodeIndex] == null)
            {
                return;
            }

            // Work out the current dimension
            var dim = dimension % this.Dimensions;

            // Split our hyper-rectangle into 2 sub rectangles along the current
            // node's point on the current dimension
            var leftRect = rect.Clone();
            leftRect.MaxPoint[dim] = this.InternalPointArray[nodeIndex][dim];

            var rightRect = rect.Clone();
            rightRect.MinPoint[dim] = this.InternalPointArray[nodeIndex][dim];

            // Determine which side the target resides in
            var compare = target[dim].CompareTo(this.InternalPointArray[nodeIndex][dim]);

            var nearerRect = compare <= 0 ? leftRect : rightRect;
            var furtherRect = compare <= 0 ? rightRect : leftRect;

            var nearerNode = compare <= 0 ? LeftChildIndex(nodeIndex) : RightChildIndex(nodeIndex);
            var furtherNode = compare <= 0 ? RightChildIndex(nodeIndex) : LeftChildIndex(nodeIndex);

            // Move down into the nearer branch
            this.SearchForNearestNeighbors(
                nearerNode,
                target,
                nearerRect,
                dimension + 1,
                nearestNeighbors,
                maxSearchRadiusSquared);

            // Walk down into the further branch but only if our capacity hasn't been reached
            // OR if there's a region in the further rectangle that's closer to the target than our
            // current furtherest nearest neighbor
            var closestPointInFurtherRect = furtherRect.GetClosestPoint(target);
            var distanceSquaredToTarget = this.Metric(closestPointInFurtherRect, target);

            if (distanceSquaredToTarget.CompareTo(maxSearchRadiusSquared) <= 0)
            {
                if (nearestNeighbors.IsFull)
                {
                    if (distanceSquaredToTarget.CompareTo(nearestNeighbors.MaxPriority) < 0)
                    {
                        this.SearchForNearestNeighbors(
                            furtherNode,
                            target,
                            furtherRect,
                            dimension + 1,
                            nearestNeighbors,
                            maxSearchRadiusSquared);
                    }
                }
                else
                {
                    this.SearchForNearestNeighbors(
                        furtherNode,
                        target,
                        furtherRect,
                        dimension + 1,
                        nearestNeighbors,
                        maxSearchRadiusSquared);
                }
            }

            // Try to add the current node to our nearest neighbors list
            distanceSquaredToTarget = this.Metric(this.InternalPointArray[nodeIndex], target);
            if (distanceSquaredToTarget.CompareTo(maxSearchRadiusSquared) <= 0)
            {
                nearestNeighbors.Add(nodeIndex, distanceSquaredToTarget);
            }
        }
    }
}
