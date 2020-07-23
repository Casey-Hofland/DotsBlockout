using Unity.Entities;
using UnityEngine;

public class PaddleAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    [SerializeField] private KeyCode leftKey;
    [SerializeField] private KeyCode rightKey;
    [SerializeField] private float speed;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new KeyCodeInput { keyA = leftKey, keyB = rightKey, });
        dstManager.AddComponentData(entity, new Movement { speed = speed, });
        dstManager.AddComponentData(entity, new PaddleTag { });
    }
}

public struct PaddleTag : IComponentData { }
