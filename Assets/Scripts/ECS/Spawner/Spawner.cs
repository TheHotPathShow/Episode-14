using Unity.Entities;
using Unity.Mathematics;
using System.Collections.Generic;

public struct Spawner : IComponentData
{
    public float NextSpawnTime;
    public float SpawnRate;
    public float2 CameraSize;
    public float3 SpawnPosition;
    public int DangerLevel;
    public float Radius;
    public int Target;
    public int MaxTarget;
}