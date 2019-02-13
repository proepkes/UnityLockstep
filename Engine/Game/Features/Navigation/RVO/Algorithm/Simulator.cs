/*
 * Simulator.cs
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

using System.Collections.Generic;
using System.Threading.Tasks;
using BEPUutilities;
using FixMath.NET;

namespace Lockstep.Game.Features.Navigation.RVO.Algorithm
{
    /**
     * <summary>Defines the simulation.</summary>
     */
    internal class Simulator
    {                   
        internal IDictionary<uint, Agent> agents_;
        internal IList<Obstacle> obstacles_;
        internal KdTree kdTree_;
        internal Fix64 timeStep_;
        private Agent defaultAgent_;              
        private Fix64 globalTime_;

        internal static Simulator Instance { get; } = new Simulator();

        /**
         * <summary>Adds a new agent with default properties to the simulation.
         * </summary>
         *
         * <returns>The number of the agent, or -1 when the agent defaults have
         * not been set.</returns>
         *
         * <param name="position">The two-dimensional starting position of this
         * agent.</param>
         */
        internal void addAgent(uint id, Vector2 position, Vector2 destination)
        {
            if (defaultAgent_ == null)
            {
                return;
            }

            Agent agent = new Agent
            {             
                maxNeighbors_ = defaultAgent_.maxNeighbors_,
                MaxSpeed = defaultAgent_.MaxSpeed,
                neighborDist_ = defaultAgent_.neighborDist_,
                Position = position,
                Destination = destination,
                radius_ = defaultAgent_.radius_,
                timeHorizon_ = defaultAgent_.timeHorizon_,
                timeHorizonObst_ = defaultAgent_.timeHorizonObst_,
                Velocity = defaultAgent_.Velocity
            };
            agents_.Add(id, agent);  
        }

        internal void removeAgent(uint id)
        {
            agents_.Remove(id);
        }

        /**
         * <summary>Adds a new obstacle to the simulation.</summary>
         *
         * <returns>The number of the first vertex of the obstacle, or -1 when
         * the number of vertices is less than two.</returns>
         *
         * <param name="vertices">List of the vertices of the polygonal obstacle
         * in counterclockwise order.</param>
         *
         * <remarks>To add a "negative" obstacle, e.g. a bounding polygon around
         * the environment, the vertices should be listed in clockwise order.
         * </remarks>
         */
        internal int addObstacle(IList<Vector2> vertices)
        {
            if (vertices.Count < 2)
            {
                return -1;
            }

            int obstacleNo = obstacles_.Count;

            for (int i = 0; i < vertices.Count; ++i)
            {
                Obstacle obstacle = new Obstacle();
                obstacle.point_ = vertices[i];

                if (i != 0)
                {
                    obstacle.previous_ = obstacles_[obstacles_.Count - 1];
                    obstacle.previous_.next_ = obstacle;
                }

                if (i == vertices.Count - 1)
                {
                    obstacle.next_ = obstacles_[obstacleNo];
                    obstacle.next_.previous_ = obstacle;
                }

                obstacle.direction_ = Vector2.Normalize(vertices[(i == vertices.Count - 1 ? 0 : i + 1)] - vertices[i]);

                if (vertices.Count == 2)
                {
                    obstacle.convex_ = true;
                }
                else
                {
                    obstacle.convex_ = (RVOMath.leftOf(vertices[(i == 0 ? vertices.Count - 1 : i - 1)], vertices[i], vertices[(i == vertices.Count - 1 ? 0 : i + 1)]) >= Fix64.Zero);
                }

                obstacle.id_ = obstacles_.Count;
                obstacles_.Add(obstacle);
            }

            return obstacleNo;
        }

        /**
         * <summary>Clears the simulation.</summary>
         */
        internal void Clear()
        {
            agents_ = new Dictionary<uint, Agent>();
            defaultAgent_ = null;
            kdTree_ = new KdTree();
            obstacles_ = new List<Obstacle>();
            globalTime_ = Fix64.Zero;
            timeStep_ = 0.1m;   
        }

        /**
         * <summary>Performs a simulation step and updates the two-dimensional
         * position and two-dimensional velocity of each agent.</summary>
         *
         * <returns>The global time after the simulation step.</returns>
         */
        internal Fix64 doStep()
        {       
            kdTree_.buildAgentTree();

            Parallel.ForEach(agents_.Values, agent =>
            {
                agent.computeNeighbors();
                agent.computeNewVelocity();
            });

            Parallel.ForEach(agents_.Values, agent =>
            {
                agent.update();
            });

            globalTime_ += timeStep_;

            return globalTime_;
        }           
        
        /**
         * <summary>Returns the two-dimensional position of a specified obstacle
         * vertex.</summary>
         *
         * <returns>The two-dimensional position of the specified obstacle
         * vertex.</returns>
         *
         * <param name="vertexNo">The number of the obstacle vertex to be
         * retrieved.</param>
         */
        internal Vector2 getObstacleVertex(int vertexNo)
        {
            return obstacles_[vertexNo].point_;
        }

        /**
         * <summary>Returns the number of the obstacle vertex succeeding the
         * specified obstacle vertex in its polygon.</summary>
         *
         * <returns>The number of the obstacle vertex succeeding the specified
         * obstacle vertex in its polygon.</returns>
         *
         * <param name="vertexNo">The number of the obstacle vertex whose
         * successor is to be retrieved.</param>
         */
        internal int getNextObstacleVertexNo(int vertexNo)
        {
            return obstacles_[vertexNo].next_.id_;
        }

        /**
         * <summary>Returns the number of the obstacle vertex preceding the
         * specified obstacle vertex in its polygon.</summary>
         *
         * <returns>The number of the obstacle vertex preceding the specified
         * obstacle vertex in its polygon.</returns>
         *
         * <param name="vertexNo">The number of the obstacle vertex whose
         * predecessor is to be retrieved.</param>
         */
        internal int getPrevObstacleVertexNo(int vertexNo)
        {
            return obstacles_[vertexNo].previous_.id_;
        }

        /**
         * <summary>Returns the time step of the simulation.</summary>
         *
         * <returns>The present time step of the simulation.</returns>
         */
        internal Fix64 getTimeStep()
        {
            return timeStep_;
        }

        /**
         * <summary>Processes the obstacles that have been added so that they
         * are accounted for in the simulation.</summary>
         *
         * <remarks>Obstacles added to the simulation after this function has
         * been called are not accounted for in the simulation.</remarks>
         */
        internal void processObstacles()
        {
            kdTree_.buildObstacleTree();
        }

        /**
         * <summary>Performs a visibility query between the two specified points
         * with respect to the obstacles.</summary>
         *
         * <returns>A boolean specifying whether the two points are mutually
         * visible. Returns true when the obstacles have not been processed.
         * </returns>
         *
         * <param name="point1">The first point of the query.</param>
         * <param name="point2">The second point of the query.</param>
         * <param name="radius">The minimal distance between the line connecting
         * the two points and the obstacles in order for the points to be
         * mutually visible (optional). Must be non-negative.</param>
         */
        internal bool queryVisibility(Vector2 point1, Vector2 point2, Fix64 radius)
        {
            return kdTree_.queryVisibility(point1, point2, radius);
        }

        /**
         * <summary>Sets the default properties for any new agent that is added.
         * </summary>
         *
         * <param name="neighborDist">The default maximum distance (center point
         * to center point) to other agents a new agent takes into account in
         * the navigation. The larger this number, the longer he running time of
         * the simulation. If the number is too low, the simulation will not be
         * safe. Must be non-negative.</param>
         * <param name="maxNeighbors">The default maximum number of other agents
         * a new agent takes into account in the navigation. The larger this
         * number, the longer the running time of the simulation. If the number
         * is too low, the simulation will not be safe.</param>
         * <param name="timeHorizon">The default minimal amount of time for
         * which a new agent's velocities that are computed by the simulation
         * are safe with respect to other agents. The larger this number, the
         * sooner an agent will respond to the presence of other agents, but the
         * less freedom the agent has in choosing its velocities. Must be
         * positive.</param>
         * <param name="timeHorizonObst">The default minimal amount of time for
         * which a new agent's velocities that are computed by the simulation
         * are safe with respect to obstacles. The larger this number, the
         * sooner an agent will respond to the presence of obstacles, but the
         * less freedom the agent has in choosing its velocities. Must be
         * positive.</param>
         * <param name="radius">The default radius of a new agent. Must be
         * non-negative.</param>
         * <param name="maxSpeed">The default maximum speed of a new agent. Must
         * be non-negative.</param>
         * <param name="velocity">The default initial two-dimensional linear
         * velocity of a new agent.</param>
         */
        internal void setAgentDefaults(Fix64 neighborDist, int maxNeighbors, Fix64 timeHorizon, Fix64 timeHorizonObst, Fix64 radius, Fix64 maxSpeed)
        {
            if (defaultAgent_ == null)
            {
                defaultAgent_ = new Agent();
            }

            defaultAgent_.maxNeighbors_ = maxNeighbors;
            defaultAgent_.MaxSpeed = maxSpeed;
            defaultAgent_.neighborDist_ = neighborDist;
            defaultAgent_.radius_ = radius;
            defaultAgent_.timeHorizon_ = timeHorizon;
            defaultAgent_.timeHorizonObst_ = timeHorizonObst;
        }

        /**
         * <summary>Sets the global time of the simulation.</summary>
         *
         * <param name="globalTime">The global time of the simulation.</param>
         */
        internal void setGlobalTime(Fix64 globalTime)
        {
            globalTime_ = globalTime;
        }    

        /**
         * <summary>Sets the time step of the simulation.</summary>
         *
         * <param name="timeStep">The time step of the simulation. Must be
         * positive.</param>
         */
        internal void setTimeStep(Fix64 timeStep)
        {
            timeStep_ = timeStep;
        }

        /**
         * <summary>Constructs and initializes a simulation.</summary>
         */
        private Simulator()
        {
            Clear();
        }
    }
}
