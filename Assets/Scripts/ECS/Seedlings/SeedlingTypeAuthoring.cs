using Unity.Entities;
using UnityEngine;

public enum SeedlingType
{
    A,
    B,
    C
}


public struct SeedlingTypeData : ISharedComponentData
{
    public SeedlingType Type;
}

public class SeedlingTypeAuthoring : MonoBehaviour
{
    public SeedlingType SeedlingType;

    public class SeedlingBaker : Baker<SeedlingTypeAuthoring>
    {
        public override void Bake(SeedlingTypeAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddSharedComponent(entity,
                new SeedlingTypeData()
                {
                    Type = authoring.SeedlingType
                });
        }
    }
}