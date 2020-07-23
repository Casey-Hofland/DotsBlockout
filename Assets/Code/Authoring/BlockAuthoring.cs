using Unity.Entities;
using UnityEngine;

public class BlockAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new BlockTag { });
    }
}

public struct BlockTag : IComponentData { }