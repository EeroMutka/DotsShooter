using Unity.Entities;
using Unity.Mathematics;

public struct GameComponent : IComponentData
{
	public int FrameIndex;
	public Entity BulletPrefab;
	public Entity PlayerPrefab;
	public float3 SpawnPosition;
	public float NextSpawnTime;
	public float SpawnRate;
}

public struct BulletComponent : IComponentData
{
	public float2 Velocity;
}

public struct PlayerComponent : IComponentData
{
	public float2 Position;
	public float2 Velocity;
}

