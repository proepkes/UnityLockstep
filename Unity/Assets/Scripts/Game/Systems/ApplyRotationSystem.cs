using Assets.Scripts.Game.ComponentData;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.Scripts.Game.Systems
{
    public class ApplyRotationSystem : JobComponentSystem
    {
        [BurstCompile(FloatPrecision.Standard, FloatMode.Deterministic)]
        struct ApplyRotationJob : IJobProcessComponentData<Rotation, RotationSpeed>
        {
            public float DeltaTime;

            public void Execute(ref Rotation rotation, [ReadOnly] ref RotationSpeed rotSpeed)
            {
                // Rotate something about its up vector at the speed given by RotationSpeed.
                rotation.Value = math.mul(math.normalize(rotation.Value), quaternion.AxisAngle(math.up(), math.radians(rotSpeed.DegreesPerSecond) * DeltaTime));
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var job = new ApplyRotationJob()
            {
                DeltaTime = 0.3f
            };

            return job.Schedule(this, inputDependencies);
        }
    }
}
