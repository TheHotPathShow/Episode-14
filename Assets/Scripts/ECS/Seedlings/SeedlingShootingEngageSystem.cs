using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAfter(typeof(PlayerMovementSystem))]
public partial struct SeedlingShootingEngageSystem : ISystem
{
    private SystemHandle handle;
    private EntityManager entityManager;

    private const float SHOOT_INTERVAL = 0.1f;

    class ShootingEngageSystemCache : IComponentData
    {
        public PlayerInputSingleton InputSin;
        public Queue<float3> ShootTargets;
        public bool IsShooting;
        public float ShootCooldown;
    }

    public void OnCreate(ref SystemState state)
    {
        handle = state.SystemHandle;
        entityManager = state.EntityManager;

        state.RequireForUpdate<ShootingEngageSystemCache>();
        var inputCache = new ShootingEngageSystemCache()
        {
            InputSin = SystemAPI.ManagedAPI.GetSingleton<PlayerInputSingleton>(),
            ShootTargets = new Queue<float3>(),
            IsShooting = false,
        };
        state.EntityManager.AddComponentObject(state.SystemHandle, inputCache);

        state.RequireForUpdate<AudioEventBuffer>();
        inputCache.InputSin.input.Gameplay.Shoot.started += ShootStarted;
        inputCache.InputSin.input.Gameplay.Shoot.canceled += ShootEnded;
    }


    private void ShootStarted(InputAction.CallbackContext obj)
    {
        entityManager.GetComponentObject<ShootingEngageSystemCache>(handle).IsShooting = true;

    }

    private void ShootEnded(InputAction.CallbackContext obj)
    {
        entityManager.GetComponentObject<ShootingEngageSystemCache>(handle).IsShooting = false;

    }

    public void OnUpdate(ref SystemState state)
    {
        var cache = SystemAPI.ManagedAPI.GetSingleton<ShootingEngageSystemCache>();

        cache.ShootCooldown -= Time.deltaTime;

        if (cache.IsShooting)
        {
            while (cache.ShootCooldown <= 0.0f)
            {
                cache.ShootCooldown += SHOOT_INTERVAL;
                
                Vector3 mousePos = Input.mousePosition;
                mousePos.z = Camera.main.nearClipPlane;
                var targetPos = Camera.main.ScreenToWorldPoint(mousePos);
                cache.ShootTargets.Enqueue(targetPos);
            }
        }
        else
        {
            cache.ShootCooldown = math.max(cache.ShootCooldown, 0.0f);
            cache.ShootTargets.Clear();
            return;
        }

        var canvasSingleton = SystemAPI.GetSingleton<CanvasSystem.Singleton>();

        var audioBuffer = SystemAPI.GetSingletonBuffer<AudioEventBuffer>();

        if (cache.ShootTargets.Count > 0)
        {
            foreach (var (shoot, seedlingState, damageDealer, vel, seedling, seedlingType, transform)
                     in SystemAPI.Query<RefRW<ShootingSeedling>, RefRW<SeedlingStateData>, RefRW<DamageDealer>, RefRW<PhysicsVelocity>
                         , SeedlingData, SeedlingTypeData, LocalTransform>().WithAll<CapturedSeedling>())
            {
                if (seedlingState.ValueRO.State != SeedlingState.Captured)
                {
                    continue;
                }

                if (canvasSingleton.SlotSelected != seedlingType.Type)
                {
                    continue;
                }

                if (!cache.ShootTargets.TryDequeue(out var targetPosition))
                {
                    continue;
                }

                AudioEventType audioEventType = seedlingType.Type switch
                {
                    SeedlingType.A => AudioEventType.SeedlingAShoot,
                    SeedlingType.B => AudioEventType.SeedlingBShoot,
                    SeedlingType.C => AudioEventType.SeedlingCShoot,
                    _ => AudioEventType.SeedlingAShoot
                };

                audioBuffer.Add(new AudioEventBuffer()
                {
                    Type = audioEventType,
                });

                shoot.ValueRW.TargetPosition = targetPosition;
                seedlingState.ValueRW.State = SeedlingState.Shooting;
                vel.ValueRW.Linear = new float3(seedling.ShootingSpeed * math.normalize(targetPosition.xy - transform.Position.xy), 0.0f);
                damageDealer.ValueRW.DidDamage = false;
                damageDealer.ValueRW.Amount = seedling.Damage;
            }
        }

        cache.ShootTargets.Clear();
    }
}