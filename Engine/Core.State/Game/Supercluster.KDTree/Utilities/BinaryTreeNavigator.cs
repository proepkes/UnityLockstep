// <copyright file="BinaryTreeNavigator.cs" company="Eric Regina">
// Copyright (c) Eric Regina. All rights reserved.
// </copyright>

namespace Supercluster.KDTree.Utilities
{
    using System;

    using static BinaryTreeNavigation;

    /// <summary>
    /// Allows one to navigate a binary tree stored in an <see cref="Array"/> using familiar
    /// tree navigation concepts.
    /// </summary>
    /// <typeparam name="TPoint">The type of the individual points.</typeparam>
    /// <typeparam name="TNode">The type of the individual nodes.</typeparam>
    public class BinaryTreeNavigator<TPoint, TNode>
    {
        /// <summary>
        /// A reference to the pointArray in which the binary tree is stored in.
        /// </summary>
        private readonly TPoint[] pointArray;

        private readonly TNode[] nodeArray;

        /// <summary>
        /// The index in the pointArray that the current node resides in.
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// The left child of the current node.
        /// </summary>
        public BinaryTreeNavigator<TPoint, TNode> Left
            =>
                LeftChildIndex(this.Index) < this.pointArray.Length - 1
                    ? new BinaryTreeNavigator<TPoint, TNode>(this.pointArray, this.nodeArray, LeftChildIndex(this.Index))
                    : null;

        /// <summary>
        /// The right child of the current node.
        /// </summary>
        public BinaryTreeNavigator<TPoint, TNode> Right
               =>
                   RightChildIndex(this.Index) < this.pointArray.Length - 1
                       ? new BinaryTreeNavigator<TPoint, TNode>(this.pointArray, this.nodeArray, RightChildIndex(this.Index))
                       : null;

        /// <summary>
        /// The parent of the current node.
        /// </summary>
        public BinaryTreeNavigator<TPoint, TNode> Parent => this.Index == 0 ? null : new BinaryTreeNavigator<TPoint, TNode>(this.pointArray, this.nodeArray, ParentIndex(this.Index));

        /// <summary>
        /// The current <typeparamref name="TPoint"/>.
        /// </summary>
        public TPoint Point => this.pointArray[this.Index];

        /// <summary>
        /// The current <typeparamref name="TNode"/>
        /// </summary>
        public TNode Node => this.nodeArray[this.Index];

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryTreeNavigator{TPoint, TNode}"/> class.
        /// </summary>
        /// <param name="pointArray">The point array backing the binary tree.</param>
        /// <param name="nodeArray">The node array corresponding to the point array.</param>
        /// <param name="index">The index of the node of interest in the pointArray. If not given, the node navigator start at the 0 index (the root of the tree).</param>
        public BinaryTreeNavigator(TPoint[] pointArray, TNode[] nodeArray, int index = 0)
        {
            this.Index = index;
            this.pointArray = pointArray;
            this.nodeArray = nodeArray;
        }
    }
}
