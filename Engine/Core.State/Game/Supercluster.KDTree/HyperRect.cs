// <copyright file="HyperRect.cs" company="Eric Regina">
// Copyright (c) Eric Regina. All rights reserved.
// </copyright>

namespace Supercluster.KDTree
{
    using System;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Represents a hyper-rectangle. An N-Dimensional rectangle.
    /// </summary>
    /// <typeparam name="T">The type of "dimension" in the metric space in which the hyper-rectangle lives.</typeparam>
    public struct HyperRect<T>
        where T : IComparable<T>
    {
        /// <summary>
        /// Backing field for the <see cref="MinPoint"/> property.
        /// </summary>
        private T[] minPoint;

        /// <summary>
        /// Backing field for the <see cref="MaxPoint"/> property.
        /// </summary>
        private T[] maxPoint;

        /// <summary>
        /// The minimum point of the hyper-rectangle. One can think of this point as the
        /// bottom-left point of a 2-Dimensional rectangle.
        /// </summary>
        public T[] MinPoint
        {
            get
            {
                return this.minPoint;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                this.minPoint = new T[value.Length];
                value.CopyTo(this.minPoint, 0);
            }
        }

        /// <summary>
        /// The maximum point of the hyper-rectangle. One can think of this point as the
        /// top-right point of a 2-Dimensional rectangle.
        /// </summary>
        public T[] MaxPoint
        {
            get
            {
                return this.maxPoint;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                this.maxPoint = new T[value.Length];
                value.CopyTo(this.maxPoint, 0);
            }
        }

        /// <summary>
        /// Get a hyper rectangle which spans the entire implicit metric space.
        /// </summary>
        /// <param name="dimensions">The number of dimensions in the hyper-rectangle's metric space.</param>
        /// <param name="positiveInfinity">The smallest possible values in any given dimension.</param>
        /// <param name="negativeInfinity">The largest possible values in any given dimension.</param>
        /// <returns>The hyper-rectangle which spans the entire metric space.</returns>
        public static HyperRect<T> Infinite(int dimensions, T positiveInfinity, T negativeInfinity)
        {
            var rect = default(HyperRect<T>);

            rect.MinPoint = new T[dimensions];
            rect.MaxPoint = new T[dimensions];

            for (var dimension = 0; dimension < dimensions; dimension++)
            {
                rect.MinPoint[dimension] = negativeInfinity;
                rect.MaxPoint[dimension] = positiveInfinity;
            }

            return rect;
        }

        /// <summary>
        /// Gets the point on the rectangle that is closest to the given point.
        /// If the point is within the rectangle, then the input point is the same as the
        /// output point.f the point is outside the rectangle then the point on the rectangle
        /// that is closest to the given point is returned.
        /// </summary>
        /// <param name="toPoint">We try to find a point in or on the rectangle closest to this point.</param>
        /// <returns>The point on or in the rectangle that is closest to the given point.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] GetClosestPoint(T[] toPoint)
        {
            var closest = new T[toPoint.Length];

            for (var dimension = 0; dimension < toPoint.Length; dimension++)
            {
                if (this.minPoint[dimension].CompareTo(toPoint[dimension]) > 0)
                {
                    closest[dimension] = this.minPoint[dimension];
                }
                else if (this.maxPoint[dimension].CompareTo(toPoint[dimension]) < 0)
                {
                    closest[dimension] = this.maxPoint[dimension];
                }
                else
                {
                    // Point is within rectangle, at least on this dimension
                    closest[dimension] = toPoint[dimension];
                }
            }

            return closest;
        }

        /// <summary>
        /// Clones the <see cref="HyperRect{T}"/>.
        /// </summary>
        /// <returns>A clone of the <see cref="HyperRect{T}"/></returns>
        public HyperRect<T> Clone()
        {
            // For a discussion of why we don't implement ICloneable
            // see http://stackoverflow.com/questions/536349/why-no-icloneablet
            var rect = default(HyperRect<T>);
            rect.MinPoint = this.MinPoint;
            rect.MaxPoint = this.MaxPoint;
            return rect;
        }
    }
}
