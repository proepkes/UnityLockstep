/*
 * Agent.cs
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
using BEPUutilities;
using FixMath.NET;

namespace Lockstep.Game.Features.Navigation.RVO.Algorithm
{
    /**
     * <summary>Defines an agent in the simulation.</summary>
     */
    internal class Agent
    {
        internal readonly IList<KeyValuePair<Fix64, Agent>> AgentNeighbors = new List<KeyValuePair<Fix64, Agent>>();
        internal readonly IList<KeyValuePair<Fix64, Obstacle>> ObstacleNeighbors = new List<KeyValuePair<Fix64, Obstacle>>();
        internal readonly IList<Line> OrcaLines = new List<Line>();
        internal Vector2 Position;
        internal Vector2 PrefVelocity;
        internal Vector2 Destination;
        internal Vector2 Velocity;
        internal int maxNeighbors_ = 0;
        internal Fix64 MaxSpeed = Fix64.Zero;
        internal Fix64 neighborDist_ = Fix64.Zero;
        internal Fix64 radius_ = Fix64.Zero;
        internal Fix64 timeHorizon_ = Fix64.Zero;
        internal Fix64 timeHorizonObst_ = Fix64.Zero;

        private Vector2 newVelocity_;

        internal void CalculatePrefVelocity()
        {           
            var goalVector = Destination - Position;

            if (goalVector.LengthSquared() > Fix64.One)
            {
                goalVector = Vector2.Normalize(goalVector);
            }

            PrefVelocity = goalVector;     
        }

        /**
         * <summary>Computes the neighbors of this agent.</summary>
         */
        internal void computeNeighbors()
        {
            ObstacleNeighbors.Clear();
            Fix64 rangeSq = RVOMath.sqr(timeHorizonObst_ * MaxSpeed + radius_);
            Simulator.Instance.kdTree_.computeObstacleNeighbors(this, rangeSq);

            AgentNeighbors.Clear();

            if (maxNeighbors_ > 0)
            {
                rangeSq = RVOMath.sqr(neighborDist_);
                Simulator.Instance.kdTree_.computeAgentNeighbors(this, ref rangeSq);
            }
        }

        /**
         * <summary>Computes the new velocity of this agent.</summary>
         */
        internal void computeNewVelocity()
        {
            OrcaLines.Clear();

            Fix64 invTimeHorizonObst = Fix64.One / timeHorizonObst_;

            /* Create obstacle ORCA lines. */
            for (int i = 0; i < ObstacleNeighbors.Count; ++i)
            {

                Obstacle obstacle1 = ObstacleNeighbors[i].Value;
                Obstacle obstacle2 = obstacle1.next_;

                Vector2 relativePosition1 = obstacle1.point_ - Position;
                Vector2 relativePosition2 = obstacle2.point_ - Position;

                /*
                 * Check if velocity obstacle of obstacle is already taken care
                 * of by previously constructed obstacle ORCA lines.
                 */
                bool alreadyCovered = false;

                for (int j = 0; j < OrcaLines.Count; ++j)
                {
                    if (RVOMath.det(invTimeHorizonObst * relativePosition1 - OrcaLines[j].point, OrcaLines[j].direction) - invTimeHorizonObst * radius_ >= -RVOMath.RVO_EPSILON && RVOMath.det(invTimeHorizonObst * relativePosition2 - OrcaLines[j].point, OrcaLines[j].direction) - invTimeHorizonObst * radius_ >= -RVOMath.RVO_EPSILON)
                    {
                        alreadyCovered = true;

                        break;
                    }
                }

                if (alreadyCovered)
                {
                    continue;
                }

                /* Not yet covered. Check for collisions. */
                Fix64 distSq1 = relativePosition1.LengthSquared();
                Fix64 distSq2 = relativePosition2.LengthSquared();

                Fix64 radiusSq = RVOMath.sqr(radius_);

                Vector2 obstacleVector = obstacle2.point_ - obstacle1.point_;
                Fix64 s = Vector2.Dot(-relativePosition1, obstacleVector) / obstacleVector.LengthSquared();
                Fix64 distSqLine = (-relativePosition1 - s * obstacleVector).LengthSquared();

                Line line;

                if (s < Fix64.Zero && distSq1 <= radiusSq)
                {
                    /* Collision with left vertex. Ignore if non-convex. */
                    if (obstacle1.convex_)
                    {
                        line.point = new Vector2(Fix64.Zero, Fix64.Zero);
                        line.direction = Vector2.Normalize(new Vector2(-relativePosition1.Y, relativePosition1.X));
                        OrcaLines.Add(line);
                    }

                    continue;
                }
                else if (s > Fix64.One && distSq2 <= radiusSq)
                {
                    /*
                     * Collision with right vertex. Ignore if non-convex or if
                     * it will be taken care of by neighboring obstacle.
                     */
                    if (obstacle2.convex_ && RVOMath.det(relativePosition2, obstacle2.direction_) >= Fix64.Zero)
                    {
                        line.point = new Vector2(Fix64.Zero, Fix64.Zero);
                        line.direction = Vector2.Normalize(new Vector2(-relativePosition2.Y, relativePosition2.X));
                        OrcaLines.Add(line);
                    }

                    continue;
                }
                else if (s >= Fix64.Zero && s < Fix64.One && distSqLine <= radiusSq)
                {
                    /* Collision with obstacle segment. */
                    line.point = new Vector2(Fix64.Zero, Fix64.Zero);
                    line.direction = -obstacle1.direction_;
                    OrcaLines.Add(line);

                    continue;
                }

                /*
                 * No collision. Compute legs. When obliquely viewed, both legs
                 * can come from a single vertex. Legs extend cut-off line when
                 * non-convex vertex.
                 */

                Vector2 leftLegDirection, rightLegDirection;

                if (s < Fix64.Zero && distSqLine <= radiusSq)
                {
                    /*
                     * Obstacle viewed obliquely so that left vertex
                     * defines velocity obstacle.
                     */
                    if (!obstacle1.convex_)
                    {
                        /* Ignore obstacle. */
                        continue;
                    }

                    obstacle2 = obstacle1;

                    Fix64 leg1 = Fix64.Sqrt(distSq1 - radiusSq);
                    leftLegDirection = new Vector2(relativePosition1.X * leg1 - relativePosition1.Y * radius_, relativePosition1.X * radius_ + relativePosition1.Y * leg1) / distSq1;
                    rightLegDirection = new Vector2(relativePosition1.X * leg1 + relativePosition1.Y * radius_, -relativePosition1.X * radius_ + relativePosition1.Y * leg1) / distSq1;
                }
                else if (s > Fix64.One && distSqLine <= radiusSq)
                {
                    /*
                     * Obstacle viewed obliquely so that
                     * right vertex defines velocity obstacle.
                     */
                    if (!obstacle2.convex_)
                    {
                        /* Ignore obstacle. */
                        continue;
                    }

                    obstacle1 = obstacle2;

                    Fix64 leg2 = Fix64.Sqrt(distSq2 - radiusSq);
                    leftLegDirection = new Vector2(relativePosition2.X * leg2 - relativePosition2.Y * radius_, relativePosition2.X * radius_ + relativePosition2.Y * leg2) / distSq2;
                    rightLegDirection = new Vector2(relativePosition2.X * leg2 + relativePosition2.Y * radius_, -relativePosition2.X * radius_ + relativePosition2.Y * leg2) / distSq2;
                }
                else
                {
                    /* Usual situation. */
                    if (obstacle1.convex_)
                    {
                        Fix64 leg1 = Fix64.Sqrt(distSq1 - radiusSq);
                        leftLegDirection = new Vector2(relativePosition1.X * leg1 - relativePosition1.Y * radius_, relativePosition1.X * radius_ + relativePosition1.Y * leg1) / distSq1;
                    }
                    else
                    {
                        /* Left vertex non-convex; left leg extends cut-off line. */
                        leftLegDirection = -obstacle1.direction_;
                    }

                    if (obstacle2.convex_)
                    {
                        Fix64 leg2 = Fix64.Sqrt(distSq2 - radiusSq);
                        rightLegDirection = new Vector2(relativePosition2.X * leg2 + relativePosition2.Y * radius_, -relativePosition2.X * radius_ + relativePosition2.Y * leg2) / distSq2;
                    }
                    else
                    {
                        /* Right vertex non-convex; right leg extends cut-off line. */
                        rightLegDirection = obstacle1.direction_;
                    }
                }

                /*
                 * Legs can never point into neighboring edge when convex
                 * vertex, take cutoff-line of neighboring edge instead. If
                 * velocity projected on "foreign" leg, no constraint is added.
                 */

                Obstacle leftNeighbor = obstacle1.previous_;

                bool isLeftLegForeign = false;
                bool isRightLegForeign = false;

                if (obstacle1.convex_ && RVOMath.det(leftLegDirection, -leftNeighbor.direction_) >= Fix64.Zero)
                {
                    /* Left leg points into obstacle. */
                    leftLegDirection = -leftNeighbor.direction_;
                    isLeftLegForeign = true;
                }

                if (obstacle2.convex_ && RVOMath.det(rightLegDirection, obstacle2.direction_) <= Fix64.Zero)
                {
                    /* Right leg points into obstacle. */
                    rightLegDirection = obstacle2.direction_;
                    isRightLegForeign = true;
                }

                /* Compute cut-off centers. */
                Vector2 leftCutOff = invTimeHorizonObst * (obstacle1.point_ - Position);
                Vector2 rightCutOff = invTimeHorizonObst * (obstacle2.point_ - Position);
                Vector2 cutOffVector = rightCutOff - leftCutOff;

                /* Project current velocity on velocity obstacle. */

                /* Check if current velocity is projected on cutoff circles. */
                Fix64 t = obstacle1 == obstacle2 ? 0.5m : Vector2.Dot((Velocity - leftCutOff), cutOffVector) / cutOffVector.LengthSquared();
                Fix64 tLeft = Vector2.Dot((Velocity - leftCutOff), leftLegDirection);
                Fix64 tRight = Vector2.Dot((Velocity - rightCutOff), rightLegDirection);

                if ((t < Fix64.Zero && tLeft < Fix64.Zero) || (obstacle1 == obstacle2 && tLeft < Fix64.Zero && tRight < Fix64.Zero))
                {
                    /* Project on left cut-off circle. */
                    Vector2 unitW = Vector2.Normalize(Velocity - leftCutOff);

                    line.direction = new Vector2(unitW.Y, -unitW.X);
                    line.point = leftCutOff + radius_ * invTimeHorizonObst * unitW;
                    OrcaLines.Add(line);

                    continue;
                }
                else if (t > Fix64.One && tRight < Fix64.Zero)
                {
                    /* Project on right cut-off circle. */
                    Vector2 unitW = Vector2.Normalize(Velocity - rightCutOff);

                    line.direction = new Vector2(unitW.Y, -unitW.X);
                    line.point = rightCutOff + radius_ * invTimeHorizonObst * unitW;
                    OrcaLines.Add(line);

                    continue;
                }

                /*
                 * Project on left leg, right leg, or cut-off line, whichever is
                 * closest to velocity.
                 */
                Fix64 distSqCutoff = (t < Fix64.Zero || t > Fix64.One || obstacle1 == obstacle2) ? Fix64.MaxValue : (Velocity - (leftCutOff + t * cutOffVector)).LengthSquared();
                Fix64 distSqLeft = tLeft < Fix64.Zero ? Fix64.MaxValue : (Velocity - (leftCutOff + tLeft * leftLegDirection)).LengthSquared();
                Fix64 distSqRight = tRight < Fix64.Zero ? Fix64.MaxValue : (Velocity - (rightCutOff + tRight * rightLegDirection)).LengthSquared();

                if (distSqCutoff <= distSqLeft && distSqCutoff <= distSqRight)
                {
                    /* Project on cut-off line. */
                    line.direction = -obstacle1.direction_;
                    line.point = leftCutOff + radius_ * invTimeHorizonObst * new Vector2(-line.direction.Y, line.direction.X);
                    OrcaLines.Add(line);

                    continue;
                }

                if (distSqLeft <= distSqRight)
                {
                    /* Project on left leg. */
                    if (isLeftLegForeign)
                    {
                        continue;
                    }

                    line.direction = leftLegDirection;
                    line.point = leftCutOff + radius_ * invTimeHorizonObst * new Vector2(-line.direction.Y, line.direction.X);
                    OrcaLines.Add(line);

                    continue;
                }

                /* Project on right leg. */
                if (isRightLegForeign)
                {
                    continue;
                }

                line.direction = -rightLegDirection;
                line.point = rightCutOff + radius_ * invTimeHorizonObst * new Vector2(-line.direction.Y, line.direction.X);
                OrcaLines.Add(line);
            }

            int numObstLines = OrcaLines.Count;

            Fix64 invTimeHorizon = Fix64.One / timeHorizon_;

            /* Create agent ORCA lines. */
            for (int i = 0; i < AgentNeighbors.Count; ++i)
            {
                Agent other = AgentNeighbors[i].Value;

                Vector2 relativePosition = other.Position - Position;
                Vector2 relativeVelocity = Velocity - other.Velocity;
                Fix64 distSq = relativePosition.LengthSquared();
                Fix64 combinedRadius = radius_ + other.radius_;
                Fix64 combinedRadiusSq = RVOMath.sqr(combinedRadius);

                Line line;
                Vector2 u;

                if (distSq > combinedRadiusSq)
                {
                    /* No collision. */
                    Vector2 w = relativeVelocity - invTimeHorizon * relativePosition;

                    /* Vector from cutoff center to relative velocity. */
                    Fix64 wLengthSq = w.LengthSquared();
                    Fix64 dotProduct1 = Vector2.Dot(w, relativePosition);

                    if (dotProduct1 < Fix64.Zero && RVOMath.sqr(dotProduct1) > combinedRadiusSq * wLengthSq)
                    {
                        /* Project on cut-off circle. */
                        Fix64 wLength = Fix64.Sqrt(wLengthSq);
                        Vector2 unitW = w / wLength;

                        line.direction = new Vector2(unitW.Y, -unitW.X);
                        u = (combinedRadius * invTimeHorizon - wLength) * unitW;
                    }
                    else
                    {
                        /* Project on legs. */
                        Fix64 leg = Fix64.Sqrt(distSq - combinedRadiusSq);

                        if (RVOMath.det(relativePosition, w) > Fix64.Zero)
                        {
                            /* Project on left leg. */
                            line.direction = new Vector2(relativePosition.X * leg - relativePosition.Y * combinedRadius, relativePosition.X * combinedRadius + relativePosition.Y * leg) / distSq;
                        }
                        else
                        {
                            /* Project on right leg. */
                            line.direction = -new Vector2(relativePosition.X * leg + relativePosition.Y * combinedRadius, -relativePosition.X * combinedRadius + relativePosition.Y * leg) / distSq;
                        }

                        Fix64 dotProduct2 = Vector2.Dot(relativeVelocity, line.direction);
                        u = dotProduct2 * line.direction - relativeVelocity;
                    }
                }
                else
                {
                    /* Collision. Project on cut-off circle of time timeStep. */
                    Fix64 invTimeStep = Fix64.One / Simulator.Instance.timeStep_;

                    /* Vector from cutoff center to relative velocity. */
                    Vector2 w = relativeVelocity - invTimeStep * relativePosition;

                    Fix64 wLength = w.Length();
                    Vector2 unitW = w / wLength;

                    line.direction = new Vector2(unitW.Y, -unitW.X);
                    u = (combinedRadius * invTimeStep - wLength) * unitW;
                }

                line.point = Velocity + 0.5m * u;
                OrcaLines.Add(line);
            }

            int lineFail = linearProgram2(OrcaLines, MaxSpeed, PrefVelocity, false, ref newVelocity_);

            if (lineFail < OrcaLines.Count)
            {
                linearProgram3(OrcaLines, numObstLines, lineFail, MaxSpeed, ref newVelocity_);
            }
        }

        /**
         * <summary>Inserts an agent neighbor into the set of neighbors of this
         * agent.</summary>
         *
         * <param name="agent">A pointer to the agent to be inserted.</param>
         * <param name="rangeSq">The squared range around this agent.</param>
         */
        internal void insertAgentNeighbor(Agent agent, ref Fix64 rangeSq)
        {
            if (this != agent)
            {
                Fix64 distSq = (Position - agent.Position).LengthSquared();

                if (distSq < rangeSq)
                {
                    if (AgentNeighbors.Count < maxNeighbors_)
                    {
                        AgentNeighbors.Add(new KeyValuePair<Fix64, Agent>(distSq, agent));
                    }

                    int i = AgentNeighbors.Count - 1;

                    while (i != 0 && distSq < AgentNeighbors[i - 1].Key)
                    {
                        AgentNeighbors[i] = AgentNeighbors[i - 1];
                        --i;
                    }

                    AgentNeighbors[i] = new KeyValuePair<Fix64, Agent>(distSq, agent);

                    if (AgentNeighbors.Count == maxNeighbors_)
                    {
                        rangeSq = AgentNeighbors[AgentNeighbors.Count - 1].Key;
                    }
                }
            }
        }

        /**
         * <summary>Inserts a static obstacle neighbor into the set of neighbors
         * of this agent.</summary>
         *
         * <param name="obstacle">The number of the static obstacle to be
         * inserted.</param>
         * <param name="rangeSq">The squared range around this agent.</param>
         */
        internal void insertObstacleNeighbor(Obstacle obstacle, Fix64 rangeSq)
        {
            Obstacle nextObstacle = obstacle.next_;

            Fix64 distSq = RVOMath.DistSqPointLineSegment(obstacle.point_, nextObstacle.point_, Position);

            if (distSq < rangeSq)
            {
                ObstacleNeighbors.Add(new KeyValuePair<Fix64, Obstacle>(distSq, obstacle));

                int i = ObstacleNeighbors.Count - 1;

                while (i != 0 && distSq < ObstacleNeighbors[i - 1].Key)
                {
                    ObstacleNeighbors[i] = ObstacleNeighbors[i - 1];
                    --i;
                }
                ObstacleNeighbors[i] = new KeyValuePair<Fix64, Obstacle>(distSq, obstacle);
            }
        }

        /**
         * <summary>Updates the two-dimensional position and two-dimensional
         * velocity of this agent.</summary>
         */
        internal void update()
        {
            Velocity = newVelocity_;
            //position_ += velocity_ * Simulator.Instance.timeStep_;
        }

        /**
         * <summary>Solves a one-dimensional linear program on a specified line
         * subject to linear constraints defined by lines and a circular
         * constraint.</summary>
         *
         * <returns>True if successful.</returns>
         *
         * <param name="lines">Lines defining the linear constraints.</param>
         * <param name="lineNo">The specified line constraint.</param>
         * <param name="radius">The radius of the circular constraint.</param>
         * <param name="optVelocity">The optimization velocity.</param>
         * <param name="directionOpt">True if the direction should be optimized.
         * </param>
         * <param name="result">A reference to the result of the linear program.
         * </param>
         */
        private bool linearProgram1(IList<Line> lines, int lineNo, Fix64 radius, Vector2 optVelocity, bool directionOpt, ref Vector2 result)
        {
            Fix64 dotProduct = Vector2.Dot(lines[lineNo].point, lines[lineNo].direction);
            Fix64 discriminant = RVOMath.sqr(dotProduct) + RVOMath.sqr(radius) - lines[lineNo].point.LengthSquared();

            if (discriminant < Fix64.Zero)
            {
                /* Max speed circle fully invalidates line lineNo. */
                return false;
            }

            Fix64 sqrtDiscriminant = Fix64.Sqrt(discriminant);
            Fix64 tLeft = -dotProduct - sqrtDiscriminant;
            Fix64 tRight = -dotProduct + sqrtDiscriminant;

            for (int i = 0; i < lineNo; ++i)
            {
                Fix64 denominator = RVOMath.det(lines[lineNo].direction, lines[i].direction);
                Fix64 numerator = RVOMath.det(lines[i].direction, lines[lineNo].point - lines[i].point);

                if (Fix64.Abs(denominator) <= RVOMath.RVO_EPSILON)
                {
                    /* Lines lineNo and i are (almost) parallel. */
                    if (numerator < Fix64.Zero)
                    {
                        return false;
                    }

                    continue;
                }

                Fix64 t = numerator / denominator;

                if (denominator >= Fix64.Zero)
                {
                    /* Line i bounds line lineNo on the right. */
                    tRight = RVOMath.Min(tRight, t);
                }
                else
                {
                    /* Line i bounds line lineNo on the left. */
                    tLeft = RVOMath.Max(tLeft, t);
                }

                if (tLeft > tRight)
                {
                    return false;
                }
            }

            if (directionOpt)
            {
                /* Optimize direction. */
                if (Vector2.Dot(optVelocity, lines[lineNo].direction) > Fix64.Zero)
                {
                    /* Take right extreme. */
                    result = lines[lineNo].point + tRight * lines[lineNo].direction;
                }
                else
                {
                    /* Take left extreme. */
                    result = lines[lineNo].point + tLeft * lines[lineNo].direction;
                }
            }
            else
            {
                /* Optimize closest point. */
                Fix64 t = Vector2.Dot(lines[lineNo].direction, optVelocity - lines[lineNo].point);

                if (t < tLeft)
                {
                    result = lines[lineNo].point + tLeft * lines[lineNo].direction;
                }
                else if (t > tRight)
                {
                    result = lines[lineNo].point + tRight * lines[lineNo].direction;
                }
                else
                {
                    result = lines[lineNo].point + t * lines[lineNo].direction;
                }
            }

            return true;
        }

        /**
         * <summary>Solves a two-dimensional linear program subject to linear
         * constraints defined by lines and a circular constraint.</summary>
         *
         * <returns>The number of the line it fails on, and the number of lines
         * if successful.</returns>
         *
         * <param name="lines">Lines defining the linear constraints.</param>
         * <param name="radius">The radius of the circular constraint.</param>
         * <param name="optVelocity">The optimization velocity.</param>
         * <param name="directionOpt">True if the direction should be optimized.
         * </param>
         * <param name="result">A reference to the result of the linear program.
         * </param>
         */
        private int linearProgram2(IList<Line> lines, Fix64 radius, Vector2 optVelocity, bool directionOpt, ref Vector2 result)
        {
            if (directionOpt)
            {
                /*
                 * Optimize direction. Note that the optimization velocity is of
                 * unit length in this case.
                 */
                result = optVelocity * radius;
            }
            else if (optVelocity.LengthSquared() > RVOMath.sqr(radius))
            {
                /* Optimize closest point and outside circle. */
                result = Vector2.Normalize(optVelocity) * radius;
            }
            else
            {
                /* Optimize closest point and inside circle. */
                result = optVelocity;
            }

            for (int i = 0; i < lines.Count; ++i)
            {
                if (RVOMath.det(lines[i].direction, lines[i].point - result) > Fix64.Zero)
                {
                    /* Result does not satisfy constraint i. Compute new optimal result. */
                    Vector2 tempResult = result;
                    if (!linearProgram1(lines, i, radius, optVelocity, directionOpt, ref result))
                    {
                        result = tempResult;

                        return i;
                    }
                }
            }

            return lines.Count;
        }

        /**
         * <summary>Solves a two-dimensional linear program subject to linear
         * constraints defined by lines and a circular constraint.</summary>
         *
         * <param name="lines">Lines defining the linear constraints.</param>
         * <param name="numObstLines">Count of obstacle lines.</param>
         * <param name="beginLine">The line on which the 2-d linear program
         * failed.</param>
         * <param name="radius">The radius of the circular constraint.</param>
         * <param name="result">A reference to the result of the linear program.
         * </param>
         */
        private void linearProgram3(IList<Line> lines, int numObstLines, int beginLine, Fix64 radius, ref Vector2 result)
        {
            Fix64 distance = Fix64.Zero;

            for (int i = beginLine; i < lines.Count; ++i)
            {
                if (RVOMath.det(lines[i].direction, lines[i].point - result) > distance)
                {
                    /* Result does not satisfy constraint of line i. */
                    IList<Line> projLines = new List<Line>();
                    for (int ii = 0; ii < numObstLines; ++ii)
                    {
                        projLines.Add(lines[ii]);
                    }

                    for (int j = numObstLines; j < i; ++j)
                    {
                        Line line;

                        Fix64 determinant = RVOMath.det(lines[i].direction, lines[j].direction);

                        if (Fix64.Abs(determinant) <= RVOMath.RVO_EPSILON)
                        {
                            /* Line i and line j are parallel. */
                            if (Vector2.Dot(lines[i].direction, lines[j].direction) > Fix64.Zero)
                            {
                                /* Line i and line j point in the same direction. */
                                continue;
                            }
                            else
                            {
                                /* Line i and line j point in opposite direction. */
                                line.point = 0.5m * (lines[i].point + lines[j].point);
                            }
                        }
                        else
                        {
                            line.point = lines[i].point + (RVOMath.det(lines[j].direction, lines[i].point - lines[j].point) / determinant) * lines[i].direction;
                        }

                        line.direction = Vector2.Normalize(lines[j].direction - lines[i].direction);
                        projLines.Add(line);
                    }

                    Vector2 tempResult = result;
                    if (linearProgram2(projLines, radius, new Vector2(-lines[i].direction.Y, lines[i].direction.X), true, ref result) < projLines.Count)
                    {
                        /*
                         * This should in principle not happen. The result is by
                         * definition already in the feasible region of this
                         * linear program. If it fails, it is due to small
                         * Fix64ing point error, and the current result is kept.
                         */
                        result = tempResult;
                    }

                    distance = RVOMath.det(lines[i].direction, lines[i].point - result);
                }
            }
        }
    }
}
