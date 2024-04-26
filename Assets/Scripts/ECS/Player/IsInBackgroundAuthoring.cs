using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem;
using RaycastHit = Unity.Physics.RaycastHit;

public class IsInBackgroundAuthoring : MonoBehaviour
{
    public float layerOffset = -9f;
    
    public class IsInBackgroundBaker : Baker<IsInBackgroundAuthoring>
    {
        public override void Bake(IsInBackgroundAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new IsInBackground
            {
                LayerOffset = authoring.layerOffset
            });
        }
    }
}

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(PhysicsSystemGroup))]
partial struct FarmingSystem : ISystem
{
    private float _tick;
    private float _prevTime;
    public void OnCreate(ref SystemState state)
    {
        _tick = 3.0f;
        _prevTime = (float) SystemAPI.Time.ElapsedTime; 
        state.RequireForUpdate<CanvasSystem.Singleton>();
        state.RequireForUpdate<PhysicsWorldSingleton>();
        state.RequireForUpdate<SimulationSingleton>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var elapsedTime = SystemAPI.Time.ElapsedTime;
        var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);
        foreach (var (soil, soilEnabled, transform, e) in SystemAPI.Query<SoilTag, EnabledRefRW<SoilTag>, LocalTransform>().WithDisabled<SoilTag>().WithEntityAccess())
        {
            var instantiatedObject = UnityEngine.Object.Instantiate(soil.SeedlingCountText) as GameObject;
            var textMesh = instantiatedObject!.GetComponent<TMPro.TextMeshPro>();
            textMesh.text = "x0";
            textMesh.rectTransform.position = (Vector3)transform.Position + new Vector3(0, -0.1f, 0.1f);
            ecb.AddComponent(e, textMesh);
            soilEnabled.ValueRW = true;
        }
        
        if ((float)elapsedTime - _prevTime > _tick) 
        {
            foreach (var (soil, seedlingVariants, entity) in SystemAPI.Query<SoilTag, DynamicBuffer<SeedlingSpawnData>>().WithEntityAccess()) 
            {
                var soiledSeedlings = SystemAPI.GetBuffer<SoiledSeedling>(entity);
                if (soiledSeedlings.Length > 0)
                {
                    var seedlingEntity = soiledSeedlings[0].Seedling;
                    var seedlingType = state.EntityManager.GetSharedComponentManaged<SeedlingTypeData>(seedlingEntity).Type;
                    var seedling = state.EntityManager.Instantiate(seedlingVariants[(int)seedlingType].Prefab);
                    soiledSeedlings.Add(new SoiledSeedling{Seedling = seedling});
                    foreach (var children in SystemAPI.GetBuffer<LinkedEntityGroup>(seedling)) 
                        ecb.AddComponent<Disabled>(children.Value);
                    ecb.AddComponent<Disabled>(seedling);
                    var textMesh = SystemAPI.ManagedAPI.GetComponent<TMPro.TextMeshPro>(entity);
                    textMesh.text = $"x{soiledSeedlings.Length}";
                }
            }
            _prevTime = (float)elapsedTime;

        }
        ecb.Playback(state.EntityManager);

        // Click to put in ground
        var leftClicked = Mouse.current.leftButton.isPressed;
        var rightClicked = Mouse.current.rightButton.isPressed;
        if (!leftClicked && !rightClicked)
            return;
        
        var cam = Camera.main;
        if (cam == null)
            return;
        var screenRay = cam.ScreenPointToRay(Input.mousePosition);
        
        var collectSoil = new SoilCollector
        {
            soilTagLookup = SystemAPI.GetComponentLookup<SoilTag>()
        };
        var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
        collisionWorld.CastRay(new RaycastInput
        {
            Start =  screenRay.origin,
            End = screenRay.direction * 1000,
            Filter = CollisionFilter.Default
        }, ref collectSoil);
        
        if (collectSoil.soilEntity != Entity.Null)
        {
            var soiledSeedlings = SystemAPI.GetBuffer<SoiledSeedling>(collectSoil.soilEntity);
            var soiledLength = soiledSeedlings.Length;
            if (leftClicked)
            {
                var canvasSingleton = SystemAPI.GetSingleton<CanvasSystem.Singleton>();
                var seedlingQuery = SystemAPI.QueryBuilder().WithAll<SeedlingTypeData>().Build();
                seedlingQuery.SetSharedComponentFilter(new SeedlingTypeData
                {
                    Type = canvasSingleton.SlotSelected
                });
                var seedlings = seedlingQuery.ToEntityArray(state.WorldUpdateAllocator);
                if (seedlings.Length > 0)
                {
                    if (soiledLength > 0) {
                        var seedlingEntity = soiledSeedlings[0].Seedling;
                        var seedlingType = state.EntityManager.GetSharedComponentManaged<SeedlingTypeData>(seedlingEntity).Type;
                        if (canvasSingleton.SlotSelected == seedlingType){ 
                            soiledSeedlings.Add(new SoiledSeedling{Seedling = seedlings[0]});
                            foreach (var children in SystemAPI.GetBuffer<Child>(seedlings[0])) 
                                state.EntityManager.AddComponent<Disabled>(children.Value);
                            state.EntityManager.AddComponent<Disabled>(seedlings[0]);
                            soiledLength++;   
                        }
                    }
                    else 
                    {
                        soiledSeedlings.Add(new SoiledSeedling{Seedling = seedlings[0]});
                        foreach (var children in SystemAPI.GetBuffer<Child>(seedlings[0])) 
                            state.EntityManager.AddComponent<Disabled>(children.Value);
                        state.EntityManager.AddComponent<Disabled>(seedlings[0]);
                        soiledLength++;   
                    }
                }
            }
            else
            {
                if (soiledSeedlings.Length > 0)
                {
                    var seedling = soiledSeedlings[0].Seedling;
                    soiledSeedlings.RemoveAt(0);
                    SystemAPI.SetComponent(seedling, SystemAPI.GetComponent<LocalTransform>(collectSoil.soilEntity));
                    state.EntityManager.SetEnabled(seedling, true);
                    foreach (var children in SystemAPI.GetBuffer<Child>(seedling)) 
                        state.EntityManager.RemoveComponent<Disabled>(children.Value);
                    state.EntityManager.RemoveComponent<Disabled>(seedling);
                    soiledLength--;
                    
                    var audioBuffer = SystemAPI.GetSingletonBuffer<AudioEventBuffer>();
                    audioBuffer.Add(new AudioEventBuffer()
                    {
                        Type = AudioEventType.Harvest
                    });
                }
            }
            
            var textMesh = SystemAPI.ManagedAPI.GetComponent<TMPro.TextMeshPro>(collectSoil.soilEntity);
            textMesh.text = $"x{soiledLength}";
        }
    }
}


struct SoilCollector : ICollector<RaycastHit>
{
    public bool EarlyOutOnFirstHit => true;
    public float MaxFraction => 1f;
    public int NumHits => 1;
    
    public ComponentLookup<SoilTag> soilTagLookup;
    public Entity soilEntity;
    public bool AddHit(RaycastHit hit)
    {
        var hasSoil = soilTagLookup.HasComponent(hit.Entity);
        if (hasSoil)
            soilEntity = hit.Entity;
        return hasSoil;
    }
}

public struct SoilTag : IComponentData, IEnableableComponent
{
    public UnityObjectRef<GameObject> SeedlingCountText;
}

public struct SoiledSeedling : IBufferElementData
{
    public Entity Seedling;
}