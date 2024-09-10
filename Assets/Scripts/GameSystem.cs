using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;

[BurstCompile]
public partial struct GameSystem : ISystem
{
	public void OnCreate(ref SystemState state) {
	}
	
	public void OnDestroy(ref SystemState state) { }
	
	public void UpdatePlayer(ref SystemState state, Entity playerEntity)
	{
		float moveX = 0f;
		float moveY = 0f;
		if (Input.GetKey(KeyCode.RightArrow)) {
			moveX += 1f;
		}
		if (Input.GetKey(KeyCode.LeftArrow)) {
			moveX -= 1f;
		}
		if (Input.GetKey(KeyCode.UpArrow)) {
			moveY += 1f;
		}
		if (Input.GetKey(KeyCode.DownArrow)) {
			moveY -= 1f;
		}
		
		float speed = 10f;
		
		LocalTransform playerTransform = state.EntityManager.GetComponentData<LocalTransform>(playerEntity);
		playerTransform.Position = playerTransform.Position + new float3(moveX, 0, moveY) * SystemAPI.Time.DeltaTime * speed;
		// LocalTransform playerTransform = LocalTransform.FromPosition(new float3(5, 0, 0));
		state.EntityManager.SetComponentData<LocalTransform>(playerEntity, playerTransform);
		
		Debug.Log("Hello, sailor!");
	}
	
	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		
		RefRW<GameComponent> game = new RefRW<GameComponent>();
		foreach (RefRW<GameComponent> gameIter in SystemAPI.Query<RefRW<GameComponent>>()) {
			game = gameIter;
			// Debug.Log("...setting game");
		}
		
		// Instantiating entities doesn't seem to work inside OnCreate, so spawn during the first frame instead
		int frameIndex = game.ValueRW.FrameIndex;
		game.ValueRW.FrameIndex += 1;
		
		if (frameIndex == 0) {
			Entity playerEntity = state.EntityManager.Instantiate(game.ValueRO.PlayerPrefab);
			state.EntityManager.AddComponent<PlayerComponent>(playerEntity);
			
			LocalTransform playerTransform = LocalTransform.FromPosition(new float3(0, 0, 0));
			state.EntityManager.SetComponentData<LocalTransform>(playerEntity, playerTransform);
		}
		
		// foreach (RefRW<PlayerComponent> playerComponent in SystemAPI.Query<RefRW<PlayerComponent>>()) {
		foreach (var (playerComponent, playerEntity) in SystemAPI.Query<RefRW<PlayerComponent>>().WithEntityAccess()) {
			UpdatePlayer(ref state, playerEntity);
			// UpdatePlayer(ref state, playerEntity);
		}
	}

	/*private void ProcessSpawner(ref SystemState state, RefRW<GameComponent> game)
	{
		// If the next spawn time has passed.
		if (game.ValueRO.NextSpawnTime < SystemAPI.Time.ElapsedTime)
		{
			// Spawns a new entity and positions it at the game.
			Entity newEntity = state.EntityManager.Instantiate(game.ValueRO.Prefab);
			// LocalPosition.FromPosition returns a Transform initialized with the given position.
			
			LocalTransform test = LocalTransform.FromPosition(game.ValueRO.SpawnPosition);
			state.EntityManager.SetComponentData<LocalTransform>(newEntity, test);

			// Resets the next spawn time.
			game.ValueRW.NextSpawnTime = (float)SystemAPI.Time.ElapsedTime + game.ValueRO.SpawnRate;
		}
	}*/
}