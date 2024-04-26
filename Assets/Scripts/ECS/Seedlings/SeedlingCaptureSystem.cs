using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(PhysicsSystemGroup))]
public partial struct SeedlingCaptureSystem : ISystem
{
    private uint randomSeed;
    private Random random;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PhysicsWorldSingleton>();
        randomSeed = 123;
        random = new Random(randomSeed);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;

        foreach (var (seedlingCapture, transform, entity) in SystemAPI.Query<SeedlingCaptureParent, LocalTransform>().WithEntityAccess())
        {
            var seedlingResolver = new SeedlingCaptureJob
            {
                SeedlingLookup = SystemAPI.GetComponentLookup<SeedlingData>(true),
                DefenderLookup = seedlingCapture,
                DefenderEntity = entity,
                SeedlingStateLookup = SystemAPI.GetComponentLookup<SeedlingStateData>(),
                velocityLookup = SystemAPI.GetComponentLookup<PhysicsVelocity>(),
                capturedSeedlingLookup = SystemAPI.GetComponentLookup<CapturedSeedling>(),
                Random = random,
            };

            collisionWorld.OverlapSphereCustom(transform.Position, 5f, ref seedlingResolver, new CollisionFilter
            {
                BelongsTo = EnemyColliderBakeSystem.ENEMY_LAYER,
                CollidesWith = EnemyColliderBakeSystem.SEEDLING_LAYER
            });
        }

    }
}


public struct SeedlingCaptureJob : ICollector<DistanceHit>
{
    public ComponentLookup<SeedlingData> SeedlingLookup;
    public SeedlingCaptureParent DefenderLookup;
    public Entity DefenderEntity;
    public ComponentLookup<SeedlingStateData> SeedlingStateLookup;
    public ComponentLookup<CapturedSeedling> capturedSeedlingLookup;
    public ComponentLookup<PhysicsVelocity> velocityLookup;
    public Random Random;
    
    void CaptureSeedling(Entity seedling, float speed)
    {
        //Debug.Log($"seedling {seedling}, follow: {followTarget} with speed: {speed}-{range}");
        SeedlingStateLookup[seedling] = new SeedlingStateData {State = SeedlingState.Captured};
        capturedSeedlingLookup[seedling] =
            new CapturedSeedling
            {
                FollowTarget = DefenderEntity,
                FollowRange = DefenderLookup.SeedlingHoldRange
            };
        velocityLookup[seedling] =
            new PhysicsVelocity
            {
                Linear = new float3(Random.NextFloat2Direction() * speed, 0.0f)
            };
    }

    public bool EarlyOutOnFirstHit => false;
    public float MaxFraction => 1.0f;
    public int NumHits { get; private set; }
    public bool AddHit(DistanceHit hit)
    {
        if (SeedlingStateLookup[hit.Entity].State == SeedlingState.Free)
        {
            CaptureSeedling(hit.Entity, SeedlingLookup[hit.Entity].CapturedStateSpeed);
            NumHits++;
            return true;
        }

        return false;
    }
}