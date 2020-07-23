using Unity.Entities;
using UnityEngine;

[RequireComponent(typeof(MaterialPropertyColorAuthoring))]
public class BallAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    [SerializeField] private float speedIncreasePerSecond;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new SpeedIncreaseOverTime { increasePerSecond = speedIncreasePerSecond, });
        dstManager.AddComponentData(entity, new BallTag { });
    }
}

public struct BallTag : IComponentData { }