using Unity.Entities;
using UnityEngine;

public partial struct DamageDealer : IComponentData
{
    public float Amount;
    public bool DidDamage;
}

public class DamageDealerAuthoring : MonoBehaviour
{
    public class DamageDealerBaker : Baker<DamageDealerAuthoring>
    {
        public override void Bake(DamageDealerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new DamageDealer());
        }
    }
}