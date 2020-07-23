using Unity.Entities;
using UnityEngine;

public class MaterialPropertyColorAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    [SerializeField] private Color color = Color.white;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new MaterialPropertyColor { Value = (Vector4)color });
    }
}
