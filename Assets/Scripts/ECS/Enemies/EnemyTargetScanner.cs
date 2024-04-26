using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(PhysicsSystemGroup))]
public partial struct EnemyTargetScanner : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PhysicsWorldSingleton>();
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;

        foreach (var (enemyRef, attackData, transform) in SystemAPI.Query<RefRW<EnemyMovementInstructions>,RefRW<EnemyAttackData>, LocalTransform>())
        {
            var enemySetTargetCollector = new EnemySetTargetCollector
            {
                EnemyMovementInstructions = enemyRef,
                AttackData = attackData
            };
            collisionWorld.OverlapSphereCustom(transform.Position, 8, ref enemySetTargetCollector, new CollisionFilter
            {
                BelongsTo = EnemyColliderBakeSystem.ENEMY_LAYER,
                CollidesWith = EnemyColliderBakeSystem.SEEDLING_LAYER
            });
        }
    }
}

[BurstCompile]
public struct EnemySetTargetCollector : ICollector<DistanceHit>
{
    public RefRW<EnemyMovementInstructions> EnemyMovementInstructions;
    public RefRW<EnemyAttackData> AttackData;
    public bool EarlyOutOnFirstHit => true;
    public float MaxFraction => 1.0f;
    public int NumHits => 1;
    public bool AddHit(DistanceHit hit)
    {
        EnemyMovementInstructions.ValueRW.Target = hit.Entity;
        AttackData.ValueRW.Target = hit.Entity;
        return true;
    }
}