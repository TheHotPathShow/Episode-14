using Unity.Entities;
using Unity.Physics;
using UnityEngine;

class LockRigidBodyAuthor : MonoBehaviour
{
    
}
class Baker : Baker<LockRigidBodyAuthor>
{
    public override void Bake(LockRigidBodyAuthor authoring)
    {
        
        var entity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent<PhysicsVelocity>(entity);
        AddComponent(entity, PhysicsMass.CreateKinematic(MassProperties.CreateSphere(1)));
    }
}
