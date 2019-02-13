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

using BEPUutilities;
using FixMath.NET;

namespace Lockstep.Game.Features.Navigation.RVO.Algorithm
{
    /**
     * <summary>Contains functions and constants used in multiple classes.
     * </summary>
     */
    internal struct RVOMath
    {
        /**
         * <summary>A sufficiently small positive number.</summary>
         */
        internal static readonly Fix64 RVO_EPSILON = 0.00001m;
        

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
        internal static Fix64 DistSqPointLineSegment(Vector2 vector1, Vector2 vector2, Vector2 vector3)
        {
            Fix64 r = Vector2.Dot(vector3 - vector1, vector2 - vector1) / (vector2 - vector1).LengthSquared();

            if (r < Fix64.Zero)
            {
                return (vector3 - vector1).LengthSquared();
            }

            if (r > Fix64.One)
            {
                return (vector3 - vector2).LengthSquared();
            }

            return (vector3 - (vector1 + r * (vector2 - vector1))).LengthSquared();
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
         * <summary>Computes the square of a Fix64.</summary>
         *
         * <returns>The square of the Fix64.</returns>
         *
         * <param name="scalar">The Fix64 to be squared.</param>
         */
        internal static Fix64 sqr(Fix64 scalar)
        {
            return scalar * scalar;
        }
        

        internal static Fix64 Min(Fix64 a, Fix64 b)
        {
            return a < b ? a : b;
        }
        internal static Fix64 Max(Fix64 a, Fix64 b)
        {
            return a > b ? a : b;
        }
    }
}
