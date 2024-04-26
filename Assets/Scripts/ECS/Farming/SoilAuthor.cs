using TMPro;
using Unity.Entities;
using Unity.Physics.Authoring;
using UnityEngine;

[RequireComponent(typeof(ForceUniqueColliderAuthoring))]
public class SoilAuthor : MonoBehaviour
{
    public TextMeshPro seedlingCountText;
    
    private class SoilAuthorBaker : Baker<SoilAuthor>
    {
        public override void Bake(SoilAuthor authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new SoilTag
            {
                SeedlingCountText = authoring.seedlingCountText.gameObject
            });
            AddBuffer<SoiledSeedling>(entity);
            SetComponentEnabled<SoilTag>(entity, false);
        }
    }
}
