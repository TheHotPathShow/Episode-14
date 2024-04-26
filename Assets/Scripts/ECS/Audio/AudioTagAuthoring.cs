using Unity.Entities;
using UnityEngine;

public struct AudioTag : IComponentData { }

public class AudioTagAuthoring : MonoBehaviour
{
    public class AudioTagBaker : Baker<AudioTagAuthoring>
    {
        public override void Bake(AudioTagAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<AudioTag>(entity);
            AddBuffer<AudioEventBuffer>(entity);
        }
    }
}