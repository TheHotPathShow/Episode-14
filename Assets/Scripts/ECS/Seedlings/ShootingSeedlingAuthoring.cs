using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct ShootingSeedling : IComponentData
{
    public float3 TargetPosition;
}

public class ShootingSeedlingAuthoring : MonoBehaviour
{
    public class ShootingSeedlingBaker : Baker<ShootingSeedlingAuthoring>
    {
        public override void Bake(ShootingSeedlingAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<ShootingSeedling>(entity);
        }
    }
}