using Unity.Entities;
using UnityEngine;

public struct DamageReceiver : IComponentData, IEnableableComponent
{
    public float Health;
    public bool IsVulnerable;
}

public struct DamageReceiveBuffer : IBufferElementData
{
    public float Damage;
}

public class DamageReceiverAuthoring : MonoBehaviour
{
    public float Health;

    public class DamageReceiverBaker : Baker<DamageReceiverAuthoring>
    {
        public override void Bake(DamageReceiverAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity,
                new DamageReceiver
                {
                    Health = authoring.Health,
                    IsVulnerable = true
                });
            AddBuffer<DamageReceiveBuffer>(entity);
        }
    }
}