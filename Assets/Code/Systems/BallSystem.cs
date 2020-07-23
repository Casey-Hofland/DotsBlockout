using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;
using Unity.Audio;
using Unity.Transforms;
using Unity.Burst;

[UpdateAfter(typeof(EndFramePhysicsSystem))]
public class BallSystem : SystemBase
{
    [BurstCompile]
    private struct BallCollisionJob : ICollisionEventsJob
    {
        [WriteOnly] public NativeList<Entity> collidedBalls;
        [ReadOnly] public ComponentDataFromEntity<BallTag> ballFromEntity;
        [ReadOnly] public ComponentDataFromEntity<BlockTag> blockFromEntity;
        [ReadOnly] public ComponentDataFromEntity<MaterialPropertyColor> colorFromEntity;
        [ReadOnly] public ComponentDataFromEntity<Score> scoreFromEntity;
        public EntityCommandBuffer entityCommandBuffer;

        public Entity uiDataEntity;
        [WriteOnly] public UIData uiData;

        public void Execute(CollisionEvent collisionEvent)
        {
            if(ballFromEntity.HasComponent(collisionEvent.EntityA))
            {
                collidedBalls.Add(collisionEvent.EntityA);

                // Change the ball's color.
                var materialPropertyColor = colorFromEntity[collisionEvent.EntityA];
                {
                    var colorZ = materialPropertyColor.Value.z;
                    materialPropertyColor.Value.z = materialPropertyColor.Value.y;
                    materialPropertyColor.Value.y = materialPropertyColor.Value.x;
                    materialPropertyColor.Value.x = colorZ;
                }
                entityCommandBuffer.SetComponent(collisionEvent.EntityA, materialPropertyColor);

                // If it hit a block, destroy it and add its score to the UIData.
                if(blockFromEntity.HasComponent(collisionEvent.EntityB))
                {
                    entityCommandBuffer.DestroyEntity(collisionEvent.EntityB);
                    uiData.score += scoreFromEntity[collisionEvent.EntityB].score;
                    entityCommandBuffer.SetComponent(uiDataEntity, uiData);
                }
            }
        }
    }

    [BurstCompile]
    private struct BallTriggerJob : ITriggerEventsJob
    {
        [WriteOnly] public NativeList<Entity> triggeredBalls;
        [ReadOnly] public ComponentDataFromEntity<BallTag> ballFromEntity;
        public EntityCommandBuffer entityCommandBuffer;

        public Entity uiDataEntity;
        [WriteOnly] public UIData uiData;

        public void Execute(TriggerEvent triggerEvent)
        {
            if(ballFromEntity.HasComponent(triggerEvent.EntityA))
            {
                triggeredBalls.Add(triggerEvent.EntityA);
                entityCommandBuffer.DestroyEntity(triggerEvent.EntityA);

                uiData.lives--;
                entityCommandBuffer.SetComponent(uiDataEntity, uiData);
            }
        }
    }

    private BuildPhysicsWorld buildPhysicsWorld;
    private StepPhysicsWorld stepPhysicsWorld;
    private EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBufferSystem;

    private AudioClip bounceClip;
    private AudioClip loseClip;

    protected override void OnCreate()
    {
        buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
        endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        bounceClip = Resources.Load<AudioClip>("Bounce");
        loseClip = Resources.Load<AudioClip>("Lose");
    }

    protected override void OnUpdate()
    {
        var ballFromEntity = GetComponentDataFromEntity<BallTag>(true);
        var entityCommandBuffer = endSimulationEntityCommandBufferSystem.CreateCommandBuffer();
        var uiDataEntity = GetSingletonEntity<UIData>();
        var uiData = GetSingleton<UIData>();

        Dependency = JobHandle.CombineDependencies(Dependency, buildPhysicsWorld.GetOutputDependency());
        Dependency = JobHandle.CombineDependencies(Dependency, stepPhysicsWorld.GetOutputDependency());

        // Check for collision event.
        var collidedBalls = new NativeList<Entity>(Allocator.TempJob);
        Dependency = new BallCollisionJob 
        { 
            collidedBalls = collidedBalls,
            ballFromEntity = ballFromEntity,
            blockFromEntity = GetComponentDataFromEntity<BlockTag>(true),
            colorFromEntity = GetComponentDataFromEntity<MaterialPropertyColor>(true),
            scoreFromEntity = GetComponentDataFromEntity<Score>(true),
            entityCommandBuffer = entityCommandBuffer,
            uiDataEntity = uiDataEntity,
            uiData = uiData,
        }.Schedule(stepPhysicsWorld.Simulation, ref buildPhysicsWorld.PhysicsWorld, Dependency);

        // Check for trigger event.
        var triggeredBalls = new NativeList<Entity>(Allocator.TempJob);
        Dependency = new BallTriggerJob
        {
            triggeredBalls = triggeredBalls,
            ballFromEntity = ballFromEntity,
            entityCommandBuffer = entityCommandBuffer,
            uiDataEntity = uiDataEntity,
            uiData = uiData,
        }.Schedule(stepPhysicsWorld.Simulation, ref buildPhysicsWorld.PhysicsWorld, Dependency);

        // Until audio can be played via Jobs, use this to play audio when an entity bounces.
        CompleteDependency();

        // Play audio.
        var translationFromEntity = GetComponentDataFromEntity<Translation>(true);
        foreach(var ball in collidedBalls)
        {
            AudioSource.PlayClipAtPoint(bounceClip, translationFromEntity[ball].Value);
        }

        foreach(var ball in triggeredBalls)
        {
            AudioSource.PlayClipAtPoint(loseClip, translationFromEntity[ball].Value);
        }

        Dependency = collidedBalls.Dispose(Dependency);
        Dependency = triggeredBalls.Dispose(Dependency);
    }
}
