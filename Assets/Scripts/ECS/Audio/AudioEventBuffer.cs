using Unity.Entities;

public enum AudioEventType
{
    SeedlingAShoot,
    SeedlingBShoot,
    SeedlingCShoot,
    Harvest,
    ChangeSelection
}

public struct AudioEventBuffer : IBufferElementData
{
    public AudioEventType Type;
}
