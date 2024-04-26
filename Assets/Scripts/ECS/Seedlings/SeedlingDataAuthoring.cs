using Unity.Entities;
using UnityEngine;

public enum SeedlingState
{
    Captured,
    Shooting,
    Free
}

public partial struct SeedlingStateData : IComponentData
{
    public SeedlingState State;
}

public partial struct SeedlingData : IComponentData
{
    public float CapturedStateSpeed;
    public float ShootingSpeed;
    public float Damage;
}

public class SeedlingDataAuthoring : MonoBehaviour
{
    public float CapturedStateSpeed;
    public float ShootingSpeed;
    public float Damage;

    public class SeedlingDataBaker : Baker<SeedlingDataAuthoring>
    {
        public override void Bake(SeedlingDataAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity,
                new SeedlingData
                {
                    CapturedStateSpeed = authoring.CapturedStateSpeed,
                    ShootingSpeed = authoring.ShootingSpeed,
                    Damage = authoring.Damage
                });

            AddComponent(entity, new SeedlingStateData {State = SeedlingState.Free});
        }
    }
}