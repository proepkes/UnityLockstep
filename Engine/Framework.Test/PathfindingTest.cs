using System;
using System.Collections.Generic;
using System.Text;
using BEPUutilities;
using FixMath.NET;
using RVO;
using Xunit;
using Xunit.Abstractions;

namespace Framework.Test
{
    public class PathfindingTest
    {
        private readonly ITestOutputHelper _output;

        public PathfindingTest(ITestOutputHelper output)
        {
            _output = output;
            Console.SetOut(new Converter(output));
        }  


        [Fact]
        public void TestRvo()
        {
            Simulator.Instance.setTimeStep(F64.C0p25);
            Simulator.Instance.setAgentDefaults(15, 10, 5, 5, 2, 2, new Vector2(0, 0));

            Simulator.Instance.addAgent(0, new Vector2(55, 55));

            var goal = new Vector2(-75, -75);

            do
            {                                                          
                for (int i = 0; i < Simulator.Instance.getNumAgents(); ++i)
                {
                    _output.WriteLine(" {0}", Simulator.Instance.getAgentPosition(i));
                }
                                      
                setPreferredVelocities(goal);
                Simulator.Instance.doStep();
            }
            while (!reachedGoal(goal));
        }
              
        void setPreferredVelocities(Vector2 goal)
        {
            for (int i = 0; i < Simulator.Instance.getNumAgents(); ++i)
            {
                Vector2 goalVector = goal - Simulator.Instance.getAgentPosition(i);

                if (RVOMath.absSq(goalVector) > Fix64.One)
                {
                    goalVector = RVOMath.normalize(goalVector);
                }

                Simulator.Instance.setAgentPrefVelocity(i, goalVector);
            }  
        }

        bool reachedGoal(Vector2 goal)
        {
            /* Check if all agents have reached their goals. */
            for (int i = 0; i < Simulator.Instance.getNumAgents(); ++i)
            {
                if (RVOMath.absSq(Simulator.Instance.getAgentPosition(i) - goal) > 400)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
