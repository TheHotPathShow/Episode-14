using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Burst;
using UnityEngine;

[BurstCompile]
public partial struct OptimizedSpawnerSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var cam = Camera.main;
        if (cam == null)
            return;

        var camRawPos = cam.transform.position;
        float2 cameraMainPosition = new float2(camRawPos.x, camRawPos.y);
        var enemyCount = SystemAPI.QueryBuilder().WithAll<EnemyMovementInstructions>().Build().CalculateEntityCount();

        // Creates a new instance of the job, assigns the necessary data, and schedules the job in parallel.
        var elapsedTime = SystemAPI.Time.ElapsedTime;

        foreach (var (spawnerRef, enemyVariants) in SystemAPI.Query<RefRW<Spawner>, DynamicBuffer<EnemySpawnData>>())
        {
            ref var spawner = ref spawnerRef.ValueRW;
            spawner.CameraSize = new float2(cam.rect.width, cam.rect.height);
            // If the next spawn time has passed.
            // Debug.Log(EnemyCount);
            var changedDangerLevel = false;
            if ((spawner.NextSpawnTime < elapsedTime) && (spawner.SpawnRate > 0.0f))
            {
                // Spawns a new entity and positions it at the spawner.
                var enemyPrefabIndex = Mathf.Min(spawner.DangerLevel - 1, enemyVariants.Length - 1);
                var newEntity = state.EntityManager.Instantiate(enemyVariants[enemyPrefabIndex].Prefab);
                var spawnPos = GetPositionOutsideOfCameraRange(spawner, (float)elapsedTime, cameraMainPosition);
                spawner.SpawnPosition = spawnPos;
                SystemAPI.SetComponent(newEntity, LocalTransform.FromPosition(spawner.SpawnPosition));

                // Resets the next spawn time.
                spawner.NextSpawnTime = (float)elapsedTime + spawner.SpawnRate;
            }
            if (spawner.DangerLevel < enemyVariants.Length)
            {
                var prevDangerLevel = spawner.DangerLevel;
                spawner.DangerLevel = GetDangerLevelBasedOnElapsedTime((float)elapsedTime);
                if (spawner.DangerLevel - prevDangerLevel > 0) 
                {
                    changedDangerLevel = true;
                }
            }
            if (changedDangerLevel) 
            {
                spawner.Target += 30;
                changedDangerLevel = false;
            }
            spawner.SpawnRate = GetSpawnRateBasedOnDangerLevel(spawner.Target, enemyCount);
        }
        
    }
    
    static int GetDangerLevelBasedOnElapsedTime(float elapsedTime) {
        int dangerLevel = Mathf.Max(1, (int)Mathf.Ceil(elapsedTime / 30.0f));
        return dangerLevel;
    }

    static float GetSpawnRateBasedOnDangerLevel(int target, int enemyCount)
    {
        var spawnRate = 0.0f;
        if (target <= enemyCount)
        {
            spawnRate = 0.0f;
        }
        else
        {
            spawnRate = 1.0f / ((float)target -  enemyCount);
        }
        return spawnRate;
    }

    static float3 GetPositionOutsideOfCameraRange(Spawner spawner, float elapsedTime, float2 cameraMainPosition)
    {
        var position = new float3(0.0f, 0.0f, 0.0f);
        while (position.x < spawner.CameraSize.x && position.x > -spawner.CameraSize.x
               && position.y < spawner.CameraSize.y && position.y > -spawner.CameraSize.y)
        {   
            position = new float3(cameraMainPosition, 0.0f);
            var randomNoise = noise.cnoise(new float3(spawner.CameraSize.x, spawner.CameraSize.y, elapsedTime));
            var randomAngle = math.remap(-1.0f, 1.0f, 0f, 360f, randomNoise);
            var posxOffset = spawner.Radius * math.cos(randomAngle);
            var posyOffset = spawner.Radius * math.sin(randomAngle);
            position += new float3(posxOffset, posyOffset, 0);
        }
        return position;
    }
}
