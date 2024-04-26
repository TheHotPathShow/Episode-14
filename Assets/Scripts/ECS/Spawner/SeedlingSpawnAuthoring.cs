using UnityEngine;
using Unity.Entities;

public class SeedlingSpawnAuthoring : MonoBehaviour 
{
    public GameObject[] Prefabs;

    public class SeedlingSpawnBaker : Baker<SeedlingSpawnAuthoring>
    {
        public override void Bake(SeedlingSpawnAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            var seedlingVariants = AddBuffer<SeedlingSpawnData>(entity);
            foreach (GameObject go in authoring.Prefabs) {
                seedlingVariants.Add(new SeedlingSpawnData
                {
                    Prefab = GetEntity(go, TransformUsageFlags.None)
                });
            }
        }
    }
}
