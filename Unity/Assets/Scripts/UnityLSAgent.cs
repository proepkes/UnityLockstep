using FixMath.NET;
using Lockstep.Framework;
using UnityEngine;                            
using Move = Lockstep.Framework.Abilities.Move;

public class UnityLSAgent : MonoBehaviour
{
    public bool Selectable = true;

    public LockstepAgent agent; 
     
    [Fix64]
    public long Speed;  

    [Fix64]
    public long Acceleration;

    public void Init(Vector3 position)
    {
        agent = new LockstepAgent(1, new BEPUutilities.Vector2((Fix64) position.x, (Fix64) position.z));   
        agent.AddAbility(new Move { Acceleration = Acceleration, Speed = Speed });
    }
          

    private void Update()
    {
        transform.position = new Vector3((float) agent.Body.Position.X, 1, (float) agent.Body.Position.Y);
    }    

    public void Simulate()
    {                                               
    }     
}
