using BEPUutilities;
using Entitas;
using FixMath.NET;
using RVO;

public class SetAgentPosition : IExecuteSystem
{
    private readonly GameContext _context;             

    public SetAgentPosition(Contexts contexts)
    {
        _context = contexts.game;        
    }     

    public void Execute()
    {
        foreach (var agent in Simulator.Instance.agents_)
        {
            var entity = _context.GetEntityWithId(agent.id_);
            entity.ReplacePosition(agent.position_);

            if (!entity.hasDestination)
            {
                return;
            }

            Vector2 goalVector = entity.destination.value - agent.position_;

            if (RVOMath.absSq(goalVector) > Fix64.One)
            {
                goalVector = RVOMath.normalize(goalVector);
            }

            agent.prefVelocity_ = goalVector;    
        }   
    }  

}     
