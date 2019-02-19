///*
// * Simulator.cs
// * RVO2 Library C#
// *
// * Copyright 2008 University of North Carolina at Chapel Hill
// *
// * Licensed under the Apache License, Version 2.0 (the "License");
// * you may not use this file except in compliance with the License.
// * You may obtain a copy of the License at
// *
// *     http://www.apache.org/licenses/LICENSE-2.0
// *
// * Unless required by applicable law or agreed to in writing, software
// * distributed under the License is distributed on an "AS IS" BASIS,
// * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// * See the License for the specific language governing permissions and
// * limitations under the License.
// *
// * Please send all bug reports to <geom@cs.unc.edu>.
// *
// * The authors may be contacted via:
// *
// * Jur van den Berg, Stephen J. Guy, Jamie Snape, Ming C. Lin, Dinesh Manocha
// * Dept. of Computer Science
// * 201 S. Columbia St.
// * Frederick P. Brooks, Jr. Computer Science Bldg.
// * Chapel Hill, N.C. 27599-3175
// * United States of America
// *
// * <http://gamma.cs.unc.edu/RVO2/>
// */

//using System.Collections.Generic;
//using System.Threading.Tasks;
//using BEPUutilities;
//using FixMath.NET;

//namespace Lockstep.Game.Features.Navigation.RVO.Algorithm
//{
//    /**
//     * <summary>Defines the simulation.</summary>
//     */
//    internal class Simulator
//    {                   
//        internal IList<Obstacle> obstacles_;
//        internal Fix64 timeStep_;  

//        internal static Simulator Instance { get; } = new Simulator();

//        /**
//         * <summary>Adds a new obstacle to the simulation.</summary>
//         *
//         * <returns>The number of the first vertex of the obstacle, or -1 when
//         * the number of vertices is less than two.</returns>
//         *
//         * <param name="vertices">List of the vertices of the polygonal obstacle
//         * in counterclockwise order.</param>
//         *
//         * <remarks>To add a "negative" obstacle, e.g. a bounding polygon around
//         * the environment, the vertices should be listed in clockwise order.
//         * </remarks>
//         */
//        internal int addObstacle(IList<Vector2> vertices)
//        {
//            if (vertices.Count < 2)
//            {
//                return -1;
//            }

//            int obstacleNo = obstacles_.Count;

//            for (int i = 0; i < vertices.Count; ++i)
//            {
//                Obstacle obstacle = new Obstacle();
//                obstacle.point_ = vertices[i];

//                if (i != 0)
//                {
//                    obstacle.previous_ = obstacles_[obstacles_.Count - 1];
//                    obstacle.previous_.next_ = obstacle;
//                }

//                if (i == vertices.Count - 1)
//                {
//                    obstacle.next_ = obstacles_[obstacleNo];
//                    obstacle.next_.previous_ = obstacle;
//                }

//                obstacle.direction_ = Vector2.Normalize(vertices[(i == vertices.Count - 1 ? 0 : i + 1)] - vertices[i]);

//                if (vertices.Count == 2)
//                {
//                    obstacle.convex_ = true;
//                }
//                else
//                {
//                    obstacle.convex_ = (RVOMath.leftOf(vertices[(i == 0 ? vertices.Count - 1 : i - 1)], vertices[i], vertices[(i == vertices.Count - 1 ? 0 : i + 1)]) >= Fix64.Zero);
//                }

//                obstacle.id_ = obstacles_.Count;
//                obstacles_.Add(obstacle);
//            }

//            return obstacleNo;
//        }

//        /**
//         * <summary>Clears the simulation.</summary>
//         */
//        internal void Clear()
//        {
//            obstacles_ = new List<Obstacle>();
//            timeStep_ = 0.1m;   
//        }      

//        internal void setTimeStep(Fix64 timeStep)
//        {
//            timeStep_ = timeStep;
//        }

//        /**
//         * <summary>Constructs and initializes a simulation.</summary>
//         */
//        private Simulator()
//        {
//            Clear();
//        }
//    }
//}
