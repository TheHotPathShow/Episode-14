using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;

struct SetRaisedTriggerEvents : IComponentData {}

partial struct SetupCollisionResponseSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
        => state.RequireForUpdate<SetRaisedTriggerEvents>();

    [BurstCompile]
    public unsafe void OnUpdate(ref SystemState state)
    {
        foreach (var colRef in SystemAPI.Query<PhysicsCollider>().WithAll<SetRaisedTriggerEvents>())
        {
            ref var collider = ref UnsafeUtility.AsRef<Collider>(colRef.ColliderPtr);
            collider.SetCollisionResponse(CollisionResponsePolicy.RaiseTriggerEvents);
        }
        state.EntityManager.RemoveComponent<SetRaisedTriggerEvents>(
            SystemAPI.QueryBuilder().WithAll<SetRaisedTriggerEvents>().Build());
    }
}

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(PhysicsSystemGroup))]

// We are updating after `PhysicsSimulationGroup` - this means that we will get the events of the current frame.
partial struct HitPlayerEventsSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        
    }
}