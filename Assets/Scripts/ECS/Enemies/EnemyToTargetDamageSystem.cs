using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateBefore(typeof(TransformSystemGroup))]
public partial struct EnemyToTargetDamageSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (enemyData, transform, attackData, entity) in
                 SystemAPI.Query<EnemyData,
                     LocalTransform,
                     RefRW<EnemyAttackData>>().WithEntityAccess())
        {
            if (attackData.ValueRW.AttackCooldown > 0)
            {
                attackData.ValueRW.AttackCooldown -= SystemAPI.Time.DeltaTime;
            }

            if (SystemAPI.Exists(attackData.ValueRO.Target))
            {
                if (!SystemAPI.HasComponent<LocalTransform>(attackData.ValueRO.Target))
                    continue;
                
                if (math.length(
                        transform.Position -
                        SystemAPI.GetComponent<LocalTransform>(attackData.ValueRO.Target).Position)
                    < enemyData.AttackRange)
                {
                    if (attackData.ValueRW.AttackCooldown <= 0)
                    {
                        state.EntityManager.GetBuffer<DamageReceiveBuffer>(attackData.ValueRO.Target)
                            .Add(new DamageReceiveBuffer {Damage = enemyData.Damage});
                        attackData.ValueRW.AttackCooldown = enemyData.AttackInterval;
                    }
                }
            }
        }
    }
}