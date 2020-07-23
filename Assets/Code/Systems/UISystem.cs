using Unity.Entities;
using UnityEngine;
using Unity.Physics.Systems;
using System.Collections;
using UnityEngine.SceneManagement;
using Unity.Scenes;
using Unity.Collections;
using Unity.Physics;
using Unity.Mathematics;

[AlwaysSynchronizeSystem]
public class UISystem : SystemBase
{
    private GameUIBinder gameUIBinder;

    public bool paused { get; private set; } = false;

    private StepPhysicsWorld stepPhysicsWorld;
    private IncreaseVelocityOverTimeSystem increaseVelocityOverTimeSystem;
    private EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBufferSystem;  // Variable and associated code not needed if HasSingleton<> and GetSingleton<> were working correctly.

    private EntityQuery blockQuery;
    private EntityQuery ballQuery;
    private Entity ball;
    private BlobAssetStore blobAssetStore;

    public void ResetSystem(GameUIBinder binder)
    {
        EntityManager.CreateEntity(typeof(UIData)); // Ideally should be placed in OnCreate, but weirdly this causes GetSingleton<> to not find it (even though it is still there in the entity debugger.)
        binder.StartCoroutine(SetGameUIBinderRoutine());

        IEnumerator SetGameUIBinderRoutine()
        {
            const int startingLives = 3;
            var uiData = GetSingleton<UIData>();
            uiData.score = default;
            uiData.lives = startingLives;
            SetSingleton(uiData);

            yield return new WaitUntil(() => blockQuery.CalculateEntityCount() > 0);
            yield return null;

            gameUIBinder = binder;

            gameUIBinder.pauseLabel.visible = paused;
            gameUIBinder.countDownLabel.text = string.Empty;
            gameUIBinder.scoreLabel.text = $"{uiData.score}";
            for(int i = 0; i < startingLives; i++)
            {
                gameUIBinder.AddLive();
            }

            SpawnBall();
        }
    }

    protected override void OnCreate()
    {
        blockQuery = EntityManager.CreateEntityQuery(ComponentType.ReadOnly<BlockTag>());
        ballQuery = EntityManager.CreateEntityQuery(ComponentType.ReadOnly<BallTag>());

        stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
        increaseVelocityOverTimeSystem = World.GetOrCreateSystem<IncreaseVelocityOverTimeSystem>();
        endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        // Create Ball Entity.
        var ballPrefab = Resources.Load<GameObject>("Ball");
        blobAssetStore = new BlobAssetStore();
        var settings = GameObjectConversionSettings.FromWorld(World, blobAssetStore);
        ball = GameObjectConversionUtility.ConvertGameObjectHierarchy(ballPrefab, settings);
    }

    protected override void OnDestroy()
    {
        if(HasSingleton<UIData>())
        {
            EntityManager.DestroyEntity(GetSingletonEntity<UIData>());
        }
        blobAssetStore.Dispose();

    }

    private WaitForSeconds waitOneSecond = new WaitForSeconds(1f);
    private WaitWhile _waitWhileSpawningBall;
    private WaitWhile waitWhileSpawningBall => _waitWhileSpawningBall ?? (_waitWhileSpawningBall = new WaitWhile(() => spawningBall));
    private bool spawningBall;
    public void SpawnBall()
    {
        gameUIBinder.StartCoroutine(SpawnBallRoutine());

        IEnumerator SpawnBallRoutine()
        {
            yield return waitWhileSpawningBall;

            spawningBall = true;

            yield return waitOneSecond;
            gameUIBinder.countDownLabel.text = "3";
            yield return waitOneSecond;
            gameUIBinder.countDownLabel.text = "2";
            yield return waitOneSecond;
            gameUIBinder.countDownLabel.text = "1";
            yield return waitOneSecond;
            gameUIBinder.countDownLabel.text = string.Empty;

            // Spawn ball with random velocity
            var xSpeed = UnityEngine.Random.Range(-0.75f, 0.75f);
            var ySpeed = Mathf.Sqrt(1 - xSpeed * xSpeed);
            const float initialSpeed = 4f;
            var ballVelocity = EntityManager.GetComponentData<PhysicsVelocity>(ball);
            ballVelocity.Linear = new float3(xSpeed, ySpeed, 0) * initialSpeed;
            //ballVelocity.Linear = new float3(0.6f, 0.8f, 0f) * initialSpeed;
            EntityManager.SetComponentData(ball, ballVelocity);
            EntityManager.Instantiate(ball);

            spawningBall = false;
        }
    }
    
    protected override void OnUpdate()
    {
        if(gameUIBinder)
        {
            var entityCommandBuffer = endSimulationEntityCommandBufferSystem.CreateCommandBuffer();

            Entities
            .WithChangeFilter<UIData>()
            .WithoutBurst()
            .ForEach((in UIData uiData) =>
            {
                gameUIBinder.scoreLabel.text = $"{uiData.score}";
                var countOffset = gameUIBinder.SetLives(uiData.lives);
                if(uiData.lives <= 0)
                {
                    entityCommandBuffer.DestroyEntity(GetSingletonEntity<UIData>());
                    SceneManager.LoadScene("LoseMenu");
                }
                else
                {
                    for(int i = 0; i > countOffset; i--)
                    {
                        SpawnBall();
                    }
                }
            }).Run();

            if(Input.GetKeyDown(KeyCode.P))
            {
                paused = !paused;

                stepPhysicsWorld.Enabled = !paused;
                increaseVelocityOverTimeSystem.Enabled = !paused;

                gameUIBinder.pauseLabel.visible = paused;
                UnityEngine.Time.timeScale = paused ? 0 : 1;
            }

            if(blockQuery.CalculateEntityCount() <= 0)
            {
                entityCommandBuffer.DestroyEntity(ballQuery);
                entityCommandBuffer.DestroyEntity(GetSingletonEntity<UIData>());
                SceneManager.LoadScene("WinMenu");
            }
        }
    }
}
