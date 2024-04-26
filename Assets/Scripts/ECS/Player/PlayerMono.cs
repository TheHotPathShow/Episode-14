using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class PlayerMono : MonoBehaviour
{
    public float Speed;
    public float SquishFrequency;
    public float2 SquishSizeStartEnd;
}

public class PlayerBaker : Baker<PlayerMono>
{
    public override void Bake(PlayerMono authoring)
    {
        var entity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(entity, new PlayerConfig
        {
            Speed = authoring.Speed,
            animatedSprite = GetEntity(
                GetComponentInChildren<AnimatedSpriteAuthor>().gameObject, TransformUsageFlags.Renderable)
        });
        AddComponent(entity, new Squishable
        {
            SquishFrequency = authoring.SquishFrequency,
            SquishSizeStartEnd = authoring.SquishSizeStartEnd
        });
    }
}

public struct CameraPosition : IComponentData 
{
    public float3 Value;
}

struct PlayerConfig : IComponentData
{
    public float Speed;
    public Entity animatedSprite;
}

struct Squishable : IComponentData
{
    public float SquishFrequency;
    public float2 SquishSizeStartEnd;
}

public class PlayerInputSingleton : IComponentData
{
    public PlayerInput input;
}

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateBefore(typeof(PhysicsSystemGroup))]
partial struct PlayerMovementSystem : ISystem
{
    private const float WORLD_BOUNDS_X = 22.4f;
    private const float WORLD_BOUNDS_Y = 10.9f;
    public void OnCreate(ref SystemState state)
    {
        var playerInput = new PlayerInput();
        playerInput.Enable();
        state.EntityManager.AddComponentObject(state.SystemHandle,
            new PlayerInputSingleton
            {
                input = playerInput
            });
    }
    
    public void OnUpdate(ref SystemState state)
    {
        var managedDataOnSystem = SystemAPI.ManagedAPI.GetComponent<PlayerInputSingleton>(state.SystemHandle);
        var direction           = new float3(managedDataOnSystem.input.Gameplay.Movement.ReadValue<Vector2>(), 0);

        foreach (var (velocityRef, playerConfig, transform) in SystemAPI.Query<RefRW<PhysicsVelocity>, PlayerConfig, LocalTransform>())
        {
            
            var targetVelocity = direction * playerConfig.Speed;
            if (transform.Position.x > WORLD_BOUNDS_X && targetVelocity.x > 0)
                targetVelocity.x = 0;
            if (transform.Position.x < -WORLD_BOUNDS_X && targetVelocity.x < 0)
                targetVelocity.x = 0;
            if (transform.Position.y > WORLD_BOUNDS_Y && targetVelocity.y > 0)
                targetVelocity.y = 0;
            if (transform.Position.y < -WORLD_BOUNDS_Y && targetVelocity.y < 0)
                targetVelocity.y = 0;
            
            // Apply movement
            velocityRef.ValueRW.Linear = targetVelocity;

            // Update animation
            ref var spriteAnimData = ref SystemAPI.GetComponentRW<SpriteCurrentAnimationSelected>(playerConfig.animatedSprite).ValueRW;
            const float epsilon = 0.01f;                                                            // Idle         | 0
            spriteAnimData.AnimationIndex =  direction is { x: 0,          y: > epsilon  } ? 1 : 0; // Up           | 1
            spriteAnimData.AnimationIndex += direction is { x: > epsilon,  y: > epsilon  } ? 1 : 0; // Up Right     | 1
            spriteAnimData.AnimationIndex += direction is { x: > epsilon,  y: 0          } ? 2 : 0; // Right        | 2
            spriteAnimData.AnimationIndex += direction is { x: > epsilon,  y: < -epsilon } ? 2 : 0; // Down Right   | 3
            spriteAnimData.AnimationIndex += direction is { x: 0,          y: < -epsilon } ? 3 : 0; // Down         | 3
            spriteAnimData.AnimationIndex += direction is { x: < -epsilon, y: < -epsilon } ? 4 : 0; // Down Left    | 3
            spriteAnimData.AnimationIndex += direction is { x: < -epsilon, y: 0          } ? 4 : 0; // Left         | 4
            spriteAnimData.AnimationIndex += direction is { x: < -epsilon, y: > epsilon  } ? 1 : 0; // Up Left      | 1
        }
    }
}

public struct IsInBackground : IComponentData
{
    public float LayerOffset;
}

[WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.Editor)]
[UpdateAfter(typeof(TransformSystemGroup))]
partial struct SquishySystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var ltwRef in SystemAPI.Query<RefRW<LocalToWorld>>().WithAll<MaterialMeshInfo>().WithNone<IsInBackground>())
        {
            ref var position = ref ltwRef.ValueRW.Value.c3;
            position.z = position.y;
        }
        
        foreach (var (ltwRef, isInBack) in SystemAPI.Query<RefRW<LocalToWorld>, IsInBackground>().WithAll<MaterialMeshInfo>())
        {
            ref var position = ref ltwRef.ValueRW.Value.c3;
            position.z = position.y + isInBack.LayerOffset;
        }
    }
}