using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using Random = Unity.Mathematics.Random;

public partial struct CapturedSeedlingMover : ISystem
{
    private Random random;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        random = new Random(345);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (seedlingState, capturedSeedling, transform, velocity, seedling, entity) in
                 SystemAPI.Query<SeedlingStateData, CapturedSeedling, LocalTransform, RefRW<PhysicsVelocity>, SeedlingData>().WithEntityAccess())
        {
            if (seedlingState.State != SeedlingState.Captured || !SystemAPI.Exists(capturedSeedling.FollowTarget))
            {
                continue;
            }

            var targetPosition = SystemAPI.GetComponent<LocalTransform>(capturedSeedling.FollowTarget).Position;
            var relPosition    = targetPosition - transform.Position;

            if (math.length(relPosition) > capturedSeedling.FollowRange
                && math.dot(math.normalize(relPosition.xy), math.normalize(velocity.ValueRO.Linear.xy)) < 0.0f)
            {
                velocity.ValueRW.Linear = new float3(random.NextFloat2Direction() * seedling.CapturedStateSpeed, 0.0f);
            }
        }
    }
}