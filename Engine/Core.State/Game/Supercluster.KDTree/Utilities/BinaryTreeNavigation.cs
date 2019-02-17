// <copyright file="BinaryTreeNavigation.cs" company="Eric Regina">
// Copyright (c) Eric Regina. All rights reserved.
// </copyright>

namespace Supercluster.KDTree.Utilities
{
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Contains methods used for doing index arithmetic to traverse nodes in a binary tree.
    /// </summary>
    public static class BinaryTreeNavigation
    {
        /// <summary>
        /// Computes the index of the right child of the current node-index.
        /// </summary>
        /// <param name="index">The index of the current node.</param>
        /// <returns>The index of the right child.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int RightChildIndex(int index)
        {
            return (2 * index) + 2;
        }

        /// <summary>
        /// Computes the index of the left child of the current node-index.
        /// </summary>
        /// <param name="index">The index of the current node.</param>
        /// <returns>The index of the left child.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int LeftChildIndex(int index)
        {
            return (2 * index) + 1;
        }

        /// <summary>
        /// Computes the index of the parent of the current node-index.
        /// </summary>
        /// <param name="index">The index of the current node.</param>
        /// <returns>The index of the parent node.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ParentIndex(int index)
        {
            return (index - 1) / 2;
        }
    }
}
