using Unity.Entities;
using UnityEngine;

public struct EnemyAttackData : IComponentData
{
    public float AttackCooldown;
    public Entity Target;
}

public class EnemyAttackDataAuthoring : MonoBehaviour
{
    public class EnemyAttackDataBaker : Baker<EnemyAttackDataAuthoring>
    {
        public override void Bake(EnemyAttackDataAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new EnemyAttackData()
            {
                Target = Entity.Null,
                AttackCooldown = 0.0f
            });
        }
    }
}