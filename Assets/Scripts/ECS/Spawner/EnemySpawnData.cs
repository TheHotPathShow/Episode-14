using Unity.Entities;
using System.Collections.Generic;


public struct EnemySpawnData : IBufferElementData
{
    public Entity Prefab;
}
