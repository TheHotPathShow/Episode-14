using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

public partial struct EnemyMovementSystem : ISystem
{
    private const float STAY_AROUND_CENTER_RANGE = 7.0f;
    private const float CENTER_CHANGE_DIR_COOLDOWN = 2f;

    private Random random;

    public void OnCreate(ref SystemState state)
    {
        random = new Random(876);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (enemyMovement, localTransform, velocityRef, attackData) in
                 SystemAPI.Query<RefRW<EnemyMovementInstructions>, LocalTransform, RefRW<PhysicsVelocity>, RefRW<EnemyAttackData>>())
        {
            ref var vel = ref velocityRef.ValueRW;
            if (SystemAPI.Exists(enemyMovement.ValueRO.Target))
            {
                var targetPos = SystemAPI.GetComponentRO<LocalTransform>(enemyMovement.ValueRO.Target).ValueRO.Position;
                var dest      = targetPos - localTransform.Position;
                if (math.lengthsq(dest) >= enemyMovement.ValueRO.BreakRange * enemyMovement.ValueRO.BreakRange)
                {
                    enemyMovement.ValueRW.Target = Entity.Null;
                    vel.Linear =
                        new float3(
                            -math.normalize(localTransform.Position.xy) *
                            enemyMovement.ValueRO.MoveSpeedPassive,
                            0.0f);
                    attackData.ValueRW.Target = Entity.Null;
                }
                else
                {
                    vel.Linear = new float3(math.normalize(dest.xy) * enemyMovement.ValueRO.MoveSpeedAggressive, 0.0f);
                }
            }
            else
            {
                if (math.length(localTransform.Position.xy) < STAY_AROUND_CENTER_RANGE)
                {
                    if (enemyMovement.ValueRO.ChangeDirCooldown <= 0.0f)
                    {
                        enemyMovement.ValueRW.ChangeDirCooldown = CENTER_CHANGE_DIR_COOLDOWN;
                        vel.Linear = new float3(random.NextFloat2Direction() * enemyMovement.ValueRO.MoveSpeedPassive, 0.0f);
                    }
                    else
                    {
                        enemyMovement.ValueRW.ChangeDirCooldown -= SystemAPI.Time.DeltaTime;
                    }

                    random.NextFloat2Direction();
                }
                else
                {
                    vel.Linear = new float3(-math.normalize(localTransform.Position.xy) * enemyMovement.ValueRO.MoveSpeedPassive, 0.0f);
                }
            }
        }
    }
}