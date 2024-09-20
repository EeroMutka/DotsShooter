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
	public float NextBulletSpawnTimer;
}

public struct ColliderComponent : IComponentData
{
	public Entity LastCollidedWith; // Null if none
}

public struct BulletComponent : IComponentData
{
	public float2 Velocity;
}

public struct PlayerComponent : IComponentData
{
	public float NextBulletSpawnTimer;
	public int Health;
}

