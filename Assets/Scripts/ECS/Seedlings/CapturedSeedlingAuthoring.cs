using Unity.Entities;
using UnityEngine;

public struct CapturedSeedling : IComponentData
{
    public Entity FollowTarget;
    public float FollowRange;
}

public class CapturedSeedlingAuthoring : MonoBehaviour
{
    public class CapturedSeedlingBaker : Baker<CapturedSeedlingAuthoring>
    {
        public override void Bake(CapturedSeedlingAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<CapturedSeedling>(entity);
        }
    }
}