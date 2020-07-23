using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

public class IncreaseVelocityOverTimeSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var deltaTime = Time.DeltaTime;
        Dependency = Entities.ForEach((ref PhysicsVelocity velocity, in SpeedIncreaseOverTime speedIncrease) =>
        {
            var direction = math.normalizesafe(velocity.Linear.xy);
            velocity.Linear.xy += direction * speedIncrease.increasePerSecond * deltaTime;
        }).Schedule(Dependency);
    }
}
