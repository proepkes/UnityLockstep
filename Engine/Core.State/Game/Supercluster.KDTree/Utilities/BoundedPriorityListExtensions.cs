// <copyright file="BoundedPriorityListExtensions.cs" company="Eric Regina">
// Copyright (c) Eric Regina. All rights reserved.
// </copyright>

namespace Supercluster.KDTree.Utilities
{
    using System;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Contains extension methods for <see cref="BoundedPriorityList{TElement,TPriority}"/> class.
    /// </summary>
    public static class BoundedPriorityListExtensions
    {
        /// <summary>
        /// Takes a <see cref="BoundedPriorityList{TElement,TPriority}"/> storing the indexes of the points and nodes of a KDTree
        /// and returns the points and nodes.
        /// </summary>
        /// <param name="list">The <see cref="BoundedPriorityList{TElement,TPriority}"/>.</param>
        /// <param name="tree">The</param>
        /// <typeparam name="TPriority">THe type of the priority of the <see cref="BoundedPriorityList{TElement,TPriority}"/></typeparam>
        /// <typeparam name="TDimension">The type of the dimensions of the <see cref="KDTree{TDimension,TNode}"/></typeparam>
        /// <typeparam name="TNode">The type of the nodes of the <see cref="KDTree{TDimension,TNode}"/></typeparam>
        /// <returns>The points and nodes in the <see cref="KDTree{TDimension,TNode}"/> implicitly referenced by the <see cref="BoundedPriorityList{TElement,TPriority}"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tuple<TDimension[], TNode>[] ToResultSet<TPriority, TDimension, TNode>(
           this BoundedPriorityList<int, TPriority> list,
           KDTree<TDimension, TNode> tree)
           where TDimension : IComparable<TDimension>
           where TPriority : IComparable<TPriority>
        {
            var array = new Tuple<TDimension[], TNode>[list.Count];
            for (var i = 0; i < list.Count; i++)
            {
                array[i] = new Tuple<TDimension[], TNode>(
                    tree.InternalPointArray[list[i]],
                    tree.InternalNodeArray[list[i]]);
            }

            return array;
        }
    }
}
