using Unity.Entities;
using Unity.Mathematics;

public struct Movement : IComponentData
{
    public float speed;
    public float3 velocity;
}
