using Unity.Entities;
using UnityEngine;

public struct FollowTarget : IComponentData
{
    public Entity Target;
}

public class FollowTargetAuthoring : MonoBehaviour
{
    public GameObject Target;

    public class FollowTargetBaker : Baker<FollowTargetAuthoring>
    {
        public override void Bake(FollowTargetAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity,
                new FollowTarget()
                {
                    Target = GetEntity(authoring.Target, TransformUsageFlags.Dynamic)
                });
        }
    }
}