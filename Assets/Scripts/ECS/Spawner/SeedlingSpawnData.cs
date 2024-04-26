using UnityEngine;
using Unity.Entities;

public struct SeedlingSpawnData : IBufferElementData
{
    public Entity Prefab;
}