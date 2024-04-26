using Unity.Entities;
using UnityEngine;

public partial struct SeedlingCaptureParent : IComponentData
{
    public float SeedlingHoldRange;
}

public class SeedlingCaptureParentAuthoring : MonoBehaviour
{
    public float SeedlingHoldRange;

    public class DefenderBaker : Baker<SeedlingCaptureParentAuthoring>
    {
        public override void Bake(SeedlingCaptureParentAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity,
                new SeedlingCaptureParent()
                {
                    SeedlingHoldRange = authoring.SeedlingHoldRange
                });
        }
    }
}