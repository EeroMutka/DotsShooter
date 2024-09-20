using Unity.Entities;
using Unity.Mathematics;

public struct GameComponent : IComponentData
{
	public int FrameIndex;
	public Entity BulletPrefab;
	public Entity PlayerPrefab;
	public Entity EnemyPrefab;
	public float3 SpawnPosition;
	public float NextEnemySpawnTime;
	public float SpawnRate;
}

public struct EnemyComponent : IComponentData
{
	// public float2 MovementTargetPosition;
	
	public float NextBulletSpawnTimer;
	// so an enemy should slowly follow the player...
	
	
	// what kind of enemy should this be?
	// It could either be a "shoot consistently towards the player" or "do a bullet burst every now and then"
}

public struct ColliderComponent : IComponentData
{
	public Entity LastCollidedWith; // Null if none
}

public struct BulletComponent : IComponentData
{
	public bool SpawnedByPlayer;
	public float2 Velocity;
}

public struct PlayerComponent : IComponentData
{
	public float NextBulletSpawnTimer;
	public int Health;
}

