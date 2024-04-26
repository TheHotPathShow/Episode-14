using Unity.Entities;
using UnityEngine;

public partial struct EnemyMovementInstructions : IComponentData
{
    public Entity Target;
    public float MoveSpeedPassive;
    public float MoveSpeedAggressive;
    public float BreakRange;
    public float ChangeDirCooldown;
}

public class EnemyMovementInstructionsAuthoring : MonoBehaviour
{
    public float MoveSpeedPassive;
    public float MoveSpeedAggressive;
    public float BreakRange;

    public class EnemyMovementInstructionsBaker : Baker<EnemyMovementInstructionsAuthoring>
    {
        public override void Bake(EnemyMovementInstructionsAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity,
                new EnemyMovementInstructions
                {
                    MoveSpeedPassive = authoring.MoveSpeedPassive,
                    MoveSpeedAggressive = authoring.MoveSpeedAggressive,
                    BreakRange = authoring.BreakRange,
                });
        }
    }
}