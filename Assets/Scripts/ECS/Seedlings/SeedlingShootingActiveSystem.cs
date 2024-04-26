using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

public partial struct SeedlingShootingActiveSystem : ISystem
{
    public const float IMPACT_RANGE = 0.1f;

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (seedlingState, shootingSeedling, transform, velocity) in SystemAPI
                     .Query<RefRW<SeedlingStateData>, ShootingSeedling, LocalTransform, RefRW<PhysicsVelocity>>())
        {
            if (seedlingState.ValueRO.State != SeedlingState.Shooting)
                continue;

            var targetVector = shootingSeedling.TargetPosition - transform.Position;
            if (math.length(targetVector) < IMPACT_RANGE ||
                math.dot(math.normalize(targetVector), math.normalize(velocity.ValueRO.Linear)) < 0.0f)
            {
                seedlingState.ValueRW.State = SeedlingState.Free;
                velocity.ValueRW.Linear = float3.zero;
            }
        }
    }
}