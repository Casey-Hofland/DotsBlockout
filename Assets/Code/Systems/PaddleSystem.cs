using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Mathematics;

// Since we will only ever have 1 paddle in the game it is cheaper to synchronize this system and run its jobs rather than schedule them.
[AlwaysSynchronizeSystem]
public class PaddleSystem : SystemBase
{
    private BuildPhysicsWorld buildPhysicsWorld;
    private CollisionFilter collisionFilter;

    protected override void OnCreate()
    {
        buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        collisionFilter = new CollisionFilter
        {
            BelongsTo = ~0u,
            CollidesWith = ~0u,
            GroupIndex = 0,
        };
    }

    protected override void OnUpdate()
    {
        var collisionWorld = buildPhysicsWorld.PhysicsWorld.CollisionWorld;
        var deltaTime = Time.DeltaTime;
        Entities.WithAll<PaddleTag>().ForEach((ref Movement movement, in KeyCodeInput input) =>
        {
            movement.velocity.x = 0;
            if(Input.GetKey(input.keyA))
            {
                movement.velocity.x -= 1;
            }
            if(Input.GetKey(input.keyB))
            {
                movement.velocity.x += 1;
            }
            movement.velocity *= movement.speed * deltaTime;
        }).Run();

        var collisionFilter = this.collisionFilter;
        var contactOffset = Physics.defaultContactOffset;
        Entities
            .WithAll<PaddleTag>()
            .WithChangeFilter<Movement>()
            .ForEach((ref Translation translation, in PhysicsCollider collider, in Movement movement) =>
        {
            if(movement.velocity.x != 0)
            {
                var aabb = collider.Value.Value.CalculateAabb();

                float3 start;
                if(movement.velocity.x > 0)
                {
                    start = translation.Value + new float3(aabb.Max.x + contactOffset, 0f, 0f);
                }
                else if(movement.velocity.x < 0)
                {
                    start = translation.Value + new float3(aabb.Min.x - contactOffset, 0f, 0f);
                }
                else
                {
                    start = default;
                }

                var raycast = new RaycastInput
                {
                    Start = start,
                    End = start + movement.velocity,
                    Filter = collisionFilter,
                };

                if(collisionWorld.CastRay(raycast, out var hit))
                {
                    translation.Value += movement.velocity * hit.Fraction;
                }
                else
                {
                    translation.Value += movement.velocity;
                }
            }
        }).Run();
    }
}
