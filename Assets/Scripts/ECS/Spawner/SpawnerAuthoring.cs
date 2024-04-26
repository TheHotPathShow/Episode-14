using UnityEngine;
using Unity.Entities;

public class SpawnerAuthoring : MonoBehaviour
{
    public GameObject[] Prefabs;
    public float SpawnRate;
    public float Radius;
    public int Target;
    public int MaxTarget;

    public class SpawnerBaker : Baker<SpawnerAuthoring>
    {
        public override void Bake(SpawnerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            
            AddComponent(entity, new Spawner
            {
                SpawnRate = authoring.SpawnRate,
                DangerLevel = 1,
                NextSpawnTime = 0.0f,
                SpawnPosition = authoring.transform.position,
                Radius = authoring.Radius,
                Target = authoring.Target,
                MaxTarget = authoring.MaxTarget
            });


            var enemyVariants = AddBuffer<EnemySpawnData>(entity);
            foreach (GameObject go in authoring.Prefabs) {
                enemyVariants.Add(new EnemySpawnData
                {
                    Prefab = GetEntity(go, TransformUsageFlags.None)
                });
            }
        }
    }
}