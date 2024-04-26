using Unity.Burst;
using Unity.Entities;

public partial struct DamageResolveSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (damageReceiver, damageReceiverEnabled, damageBuffer) in
                 SystemAPI.Query<RefRW<DamageReceiver>, EnabledRefRW<DamageReceiver>, DynamicBuffer<DamageReceiveBuffer>>())
        {
            var damage = 0.0f;
            foreach (var entry in damageBuffer)
            {
                damage += entry.Damage;
            }

            damageBuffer.Clear();
            damageReceiver.ValueRW.Health -= damage;

            if (damageReceiver.ValueRW.Health <= 0.0f)
            {
                damageReceiverEnabled.ValueRW = false;
            }
        }

        state.EntityManager.DestroyEntity(SystemAPI.QueryBuilder()
            .WithDisabled<DamageReceiver>()
            .Build().ToEntityArray(state.WorldUpdateAllocator));
    }
}