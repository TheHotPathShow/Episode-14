using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(PhysicsSystemGroup))]
public partial struct SeedlingToEnemyDamageSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SimulationSingleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        state.Dependency = new SeedlingDamageJob
        {
            SeedlingLookup = SystemAPI.GetComponentLookup<SeedlingData>(true),
            EnemyLookup = SystemAPI.GetComponentLookup<EnemyData>(true),
            DamageDealerLookup = SystemAPI.GetComponentLookup<DamageDealer>(),
            EnemyDamageBufferLookup = SystemAPI.GetBufferLookup<DamageReceiveBuffer>(),
        }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);
    }
}

[BurstCompile]
public struct SeedlingDamageJob : ITriggerEventsJob
{
    [ReadOnly] public ComponentLookup<SeedlingData> SeedlingLookup;
    [ReadOnly] public ComponentLookup<EnemyData> EnemyLookup;
    public ComponentLookup<DamageDealer> DamageDealerLookup;
    public BufferLookup<DamageReceiveBuffer> EnemyDamageBufferLookup;

    public void Execute(TriggerEvent triggerEvent)
    {
        if (SeedlingLookup.HasComponent(triggerEvent.EntityA) && EnemyLookup.HasComponent(triggerEvent.EntityB))
        {
            if (!DamageDealerLookup[triggerEvent.EntityA].DidDamage)
            {
                EnemyDamageBufferLookup[triggerEvent.EntityB].Add(new DamageReceiveBuffer {Damage = DamageDealerLookup[triggerEvent.EntityA].Amount});
                DamageDealerLookup.GetRefRW(triggerEvent.EntityA).ValueRW.DidDamage = true;
            }
        }
        else if (SeedlingLookup.HasComponent(triggerEvent.EntityB) && EnemyLookup.HasComponent(triggerEvent.EntityA))
        {
            if (!DamageDealerLookup[triggerEvent.EntityB].DidDamage)
            {
                EnemyDamageBufferLookup[triggerEvent.EntityA].Add(new DamageReceiveBuffer {Damage = DamageDealerLookup[triggerEvent.EntityB].Amount});
                DamageDealerLookup.GetRefRW(triggerEvent.EntityB).ValueRW.DidDamage = true;
            }
        }
    }
}