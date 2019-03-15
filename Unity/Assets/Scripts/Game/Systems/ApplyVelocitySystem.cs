using Assets.Scripts.Game.ComponentData;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

public class ApplyVelocitySystem : JobComponentSystem
{             
    [BurstCompile(FloatPrecision.Standard, FloatMode.Deterministic)]
    struct ApplyVelocityJob : IJobProcessComponentData<Velocity, Translation>
    {                                              
        public void Execute(
            [ReadOnly] ref Velocity v,
            ref Translation t)
        {
            t.Value += v.Value * 0.01f;
        }              
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {                                                             
        return new ApplyVelocityJob().Schedule(this, inputDeps);
    }
}
