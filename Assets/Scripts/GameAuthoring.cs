using UnityEngine;
using Unity.Entities;

class GameAuthoring : MonoBehaviour
{
	public GameObject BulletPrefab;
	public GameObject PlayerPrefab;
	public float SpawnRate;
}

class GameBaker : Baker<GameAuthoring>
{
	public override void Bake(GameAuthoring authoring)
	{
		var entity = GetEntity(TransformUsageFlags.None);
		
		AddComponent(entity, new GameComponent
		{
			// By default, each authoring GameObject turns into an Entity.
			// Given a GameObject (or authoring component), GetEntity looks up the resulting Entity.
			FrameIndex = 0,
			BulletPrefab = GetEntity(authoring.BulletPrefab, TransformUsageFlags.Dynamic),
			PlayerPrefab = GetEntity(authoring.PlayerPrefab, TransformUsageFlags.Dynamic),
			SpawnPosition = authoring.transform.position,
			NextSpawnTime = 0.0f,
			SpawnRate = authoring.SpawnRate
		});
	}
}