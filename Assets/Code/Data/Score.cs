using Unity.Entities;

[GenerateAuthoringComponent]
public struct Score : IComponentData
{
    public float score;
}
