using System;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;


public partial class CameraControllerSystem : SystemBase
{
    protected override void OnUpdate()
    {
        foreach (var (playerConfig, playerEntity) in SystemAPI.Query<PlayerConfig>().WithEntityAccess()){
            var playerPosition = SystemAPI.GetComponentRO<LocalTransform>(playerEntity).ValueRO.Position;
            var cameraSingleton = CameraMono.Instance;
            if (cameraSingleton == null) return;
            cameraSingleton.transform.position = new Vector3{
                x = playerPosition.x,
                y = playerPosition.y,
                z = -100.0f
        };
        }
    }
}