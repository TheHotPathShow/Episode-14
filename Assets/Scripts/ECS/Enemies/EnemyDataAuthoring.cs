using Unity.Entities;
using UnityEngine;

public struct EnemyData : IComponentData
{
    public float Damage;
    public float AttackInterval;
    public float AttackRange;
}

public class EnemyDataAuthoring : MonoBehaviour
{
    public float Damage;
    public float AttackInterval;
    public float AttackRange;

    public class EnemyDataBaker : Baker<EnemyDataAuthoring>
    {
        public override void Bake(EnemyDataAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity,
                new EnemyData
                {
                    Damage = authoring.Damage,
                    AttackInterval = authoring.AttackInterval,
                    AttackRange = authoring.AttackRange
                });
        }
    }
}