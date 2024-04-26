using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

public partial struct FollowTargetSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (followTarget, transform) in
                 SystemAPI.Query<FollowTarget, RefRW<LocalTransform>>())
        {
            transform.ValueRW.Position = SystemAPI.GetComponent<LocalTransform>(followTarget.Target).Position;
        }
    }
}