/*
 * RVOMath.cs
 * RVO2 Library C#
 *
 * Copyright 2008 University of North Carolina at Chapel Hill
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 * Please send all bug reports to <geom@cs.unc.edu>.
 *
 * The authors may be contacted via:
 *
 * Jur van den Berg, Stephen J. Guy, Jamie Snape, Ming C. Lin, Dinesh Manocha
 * Dept. of Computer Science
 * 201 S. Columbia St.
 * Frederick P. Brooks, Jr. Computer Science Bldg.
 * Chapel Hill, N.C. 27599-3175
 * United States of America
 *
 * <http://gamma.cs.unc.edu/RVO2/>
 */

using System;
using BEPUutilities;
using FixMath.NET;

namespace RVO
{
    /**
     * <summary>Contains functions and constants used in multiple classes.
     * </summary>
     */
    public struct RVOMath
    {
        /**
         * <summary>A sufficiently small positive number.</summary>
         */
        internal static readonly Fix64 RVO_EPSILON = F64.C0p001;

        /**
         * <summary>Computes the length of a specified two-dimensional vector.
         * </summary>
         *
         * <param name="vector">The two-dimensional vector whose length is to be
         * computed.</param>
         * <returns>The length of the two-dimensional vector.</returns>
         */
        public static Fix64 abs(Vector2 vector)
        {
            return sqrt(absSq(vector));
        }

        /**
         * <summary>Computes the squared length of a specified two-dimensional
         * vector.</summary>
         *
         * <returns>The squared length of the two-dimensional vector.</returns>
         *
         * <param name="vector">The two-dimensional vector whose squared length
         * is to be computed.</param>
         */
        public static Fix64 absSq(Vector2 vector)
        {
            return  Vector2.Dot(vector, vector);
        }

        /**
         * <summary>Computes the normalization of the specified two-dimensional
         * vector.</summary>
         *
         * <returns>The normalization of the two-dimensional vector.</returns>
         *
         * <param name="vector">The two-dimensional vector whose normalization
         * is to be computed.</param>
         */
        public static Vector2 normalize(Vector2 vector)
        {
            return vector / abs(vector);
        }

        /**
         * <summary>Computes the determinant of a two-dimensional square matrix
         * with rows consisting of the specified two-dimensional vectors.
         * </summary>
         *
         * <returns>The determinant of the two-dimensional square matrix.
         * </returns>
         *
         * <param name="vector1">The top row of the two-dimensional square
         * matrix.</param>
         * <param name="vector2">The bottom row of the two-dimensional square
         * matrix.</param>
         */
        internal static Fix64 det(Vector2 vector1, Vector2 vector2)
        {
            return vector1.X * vector2.Y - vector1.Y * vector2.X;
        }

        /**
         * <summary>Computes the squared distance from a line segment with the
         * specified endpoints to a specified point.</summary>
         *
         * <returns>The squared distance from the line segment to the point.
         * </returns>
         *
         * <param name="vector1">The first endpoint of the line segment.</param>
         * <param name="vector2">The second endpoint of the line segment.
         * </param>
         * <param name="vector3">The point to which the squared distance is to
         * be calculated.</param>
         */
        internal static Fix64 distSqPointLineSegment(Vector2 vector1, Vector2 vector2, Vector2 vector3)
        {
            Fix64 r = Vector2.Dot(vector3 - vector1, vector2 - vector1) / absSq(vector2 - vector1);

            if (r < F64.C0)
            {
                return absSq(vector3 - vector1);
            }

            if (r > F64.C1)
            {
                return absSq(vector3 - vector2);
            }

            return absSq(vector3 - (vector1 + r * (vector2 - vector1)));
        }

        /**
         * <summary>Computes the absolute value of a float.</summary>
         *
         * <returns>The absolute value of the float.</returns>
         *
         * <param name="scalar">The float of which to compute the absolute
         * value.</param>
         */
        internal static Fix64 fabs(Fix64 scalar)
        {
            return Fix64.Abs(scalar);
        }

        /**
         * <summary>Computes the signed distance from a line connecting the
         * specified points to a specified point.</summary>
         *
         * <returns>Positive when the point c lies to the left of the line ab.
         * </returns>
         *
         * <param name="a">The first point on the line.</param>
         * <param name="b">The second point on the line.</param>
         * <param name="c">The point to which the signed distance is to be
         * calculated.</param>
         */
        internal static Fix64 leftOf(Vector2 a, Vector2 b, Vector2 c)
        {
            return det(a - c, b - a);
        }

        /**
         * <summary>Computes the square of a float.</summary>
         *
         * <returns>The square of the float.</returns>
         *
         * <param name="scalar">The float to be squared.</param>
         */
        internal static Fix64 sqr(Fix64 scalar)
        {
            return Fix64.Pow2(scalar);
        }

        /**
         * <summary>Computes the square root of a float.</summary>
         *
         * <returns>The square root of the float.</returns>
         *
         * <param name="scalar">The float of which to compute the square root.
         * </param>
         */
        internal static Fix64 sqrt(Fix64 scalar)
        {
            return Fix64.Sqrt(scalar);
        }
    }
}
