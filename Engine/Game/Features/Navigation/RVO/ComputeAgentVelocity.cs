using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BEPUutilities;
using Entitas;
using FixMath.NET;
using Lockstep.Core.State.Game;
using Lockstep.Game.Features.Navigation.RVO.Algorithm;

namespace Lockstep.Game.Features.Navigation.RVO
{
    class ComputeAgentVelocity : IExecuteSystem
    {
        private readonly ConfigContext _configContext;
        private readonly IGroup<GameEntity> _movableEntities;

        public ComputeAgentVelocity(Contexts contexts, ServiceContainer services)
        {
            _configContext = contexts.config;
            _movableEntities = contexts.game.GetGroup(GameMatcher.AllOf(GameMatcher.LocalId, GameMatcher.Agent));
        }

        public void Execute()
        {
            foreach (var entity in _movableEntities)
            {
                entity.agent.orcaLines.Clear();
                Fix64 invTimeHorizon = Fix64.One / entity.agent.timeHorizon;

                /* Create agent ORCA lines. */
                foreach (var neighbor in entity.neighbors.array.Where(e => e != null))
                {
                    Vector2 relativePosition = neighbor.position.value - entity.position.value;
                    Vector2 relativeVelocity = entity.velocity.value - neighbor.velocity.value;
                    Fix64 distSq = relativePosition.LengthSquared();
                    Fix64 combinedRadius = entity.radius.value + neighbor.radius.value;
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
                                line.direction =
                                    new Vector2(relativePosition.X * leg - relativePosition.Y * combinedRadius,
                                        relativePosition.X * combinedRadius + relativePosition.Y * leg) / distSq;
                            }
                            else
                            {
                                /* Project on right leg. */
                                line.direction =
                                    -new Vector2(relativePosition.X * leg + relativePosition.Y * combinedRadius,
                                        -relativePosition.X * combinedRadius + relativePosition.Y * leg) / distSq;
                            }

                            Fix64 dotProduct2 = Vector2.Dot(relativeVelocity, line.direction);
                            u = dotProduct2 * line.direction - relativeVelocity;
                        }
                    }
                    else
                    {
                        /* Collision. Project on cut-off circle of time timeStep. */
                        Fix64 invTimeStep = Fix64.One / _configContext.navigationTimeStep.value;

                        /* Vector from cutoff center to relative velocity. */
                        Vector2 w = relativeVelocity - invTimeStep * relativePosition;

                        Fix64 wLength = w.Length();
                        Vector2 unitW = w / wLength;

                        line.direction = new Vector2(unitW.Y, -unitW.X);
                        u = (combinedRadius * invTimeStep - wLength) * unitW;
                    }

                    line.point = entity.velocity.value + 0.5m * u;
                    entity.agent.orcaLines.Add(line);
                }

                var lineFail = linearProgram2(entity.agent.orcaLines, entity.agent.maxSpeed,
                    entity.agent.preferredVelocity, false, ref entity.agent.velocity);

                if (lineFail < entity.agent.orcaLines.Count)
                {
                    linearProgram3(entity.agent.orcaLines, 0, lineFail, entity.agent.maxSpeed,
                        ref entity.agent.velocity);
                }
            }
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
