using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Physics;

[WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
[UpdateInGroup(typeof(PostBakingSystemGroup))]
public partial struct EnemyColliderBakeSystem : ISystem
{
    public const uint ENEMY_LAYER = 1 << 1;
    public const uint SEEDLING_LAYER = 1 << 2;
    public const uint PLAYER_LAYER = 1 << 3;
    public const uint ENEMYTARGET_LAYER = SEEDLING_LAYER | PLAYER_LAYER;
    public const uint SOIL_LAYER = 1 << 4;
    
    
    [BurstCompile]
    public unsafe void OnUpdate(ref SystemState state)
    {
        foreach (var colRef in SystemAPI.Query<PhysicsCollider>().WithAll<EnemyData>().WithOptions(EntityQueryOptions.IncludePrefab))
        {
            ref var collider = ref UnsafeUtility.AsRef<Collider>(colRef.ColliderPtr);
            collider.SetCollisionFilter(new CollisionFilter()
            {
                BelongsTo = ENEMY_LAYER,
                CollidesWith = ~ENEMY_LAYER,
            });
        }
        
        foreach (var colRef in SystemAPI.Query<PhysicsCollider>().WithAll<PlayerConfig>().WithOptions(EntityQueryOptions.IncludePrefab))
        {
            ref var collider = ref UnsafeUtility.AsRef<Collider>(colRef.ColliderPtr);
            collider.SetCollisionFilter(new CollisionFilter()
            {
                BelongsTo = PLAYER_LAYER,
                CollidesWith = ~ENEMYTARGET_LAYER,
            });
        }
        
        foreach (var colRef in SystemAPI.Query<PhysicsCollider>().WithAll<SeedlingData>().WithOptions(EntityQueryOptions.IncludePrefab))
        {
            ref var collider = ref UnsafeUtility.AsRef<Collider>(colRef.ColliderPtr);
            collider.SetCollisionFilter(new CollisionFilter()
            {
                BelongsTo = SEEDLING_LAYER,
                CollidesWith = ~ENEMYTARGET_LAYER,
            });
        }
        
        foreach (var colRef in SystemAPI.Query<PhysicsCollider>().WithAll<SoilTag>().WithOptions(EntityQueryOptions.IncludePrefab))
        {
            ref var collider = ref UnsafeUtility.AsRef<Collider>(colRef.ColliderPtr);
            collider.SetCollisionFilter(new CollisionFilter()
            {
                BelongsTo = SOIL_LAYER,
                CollidesWith = ~(ENEMYTARGET_LAYER|SOIL_LAYER|ENEMY_LAYER),
            });
        }
    }
}
