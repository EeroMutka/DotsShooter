using UnityEngine;
using System.Collections;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;

[BurstCompile]
public partial struct GameSystem : ISystem
{
	[BurstCompile]
	public void OnCreate(ref SystemState state) {
	}
	
	[BurstCompile]
	public void OnDestroy(ref SystemState state) { }
	
	[BurstCompile]
	public void UpdateBullet(ref SystemState state, RefRW<BulletComponent> bulletComponent, Entity bulletEntity)
	{
		LocalTransform transform = state.EntityManager.GetComponentData<LocalTransform>(bulletEntity);
		
		float2 velocity = bulletComponent.ValueRW.Velocity;
		
		transform.Position += new float3(velocity.x, 0, velocity.y) * SystemAPI.Time.DeltaTime;
		
		state.EntityManager.SetComponentData<LocalTransform>(bulletEntity, transform);
	}
	
	[BurstCompile]
	public void UpdateBulletSystem(ref SystemState state) {
		foreach (var (bulletComponent, bulletEntity) in SystemAPI.Query<RefRW<BulletComponent>>().WithEntityAccess()) {
			UpdateBullet(ref state, bulletComponent, bulletEntity);
		}
	}
	
	[BurstCompile]
	public void UpdatePlayer(ref SystemState state, RefRW<GameComponent> game, Entity playerEntity)
	{
		float moveX = 0f;
		float moveY = 0f;
		if (Input.GetKey(KeyCode.D)) {
			moveX += 1f;
		}
		if (Input.GetKey(KeyCode.A)) {
			moveX -= 1f;
		}
		if (Input.GetKey(KeyCode.W)) {
			moveY += 1f;
		}
		if (Input.GetKey(KeyCode.S)) {
			moveY -= 1f;
		}
		
		const float speed = 10f;
		
		// let's just use an orthographic camera.
		float mouseX = Input.mousePosition.x / (float)Screen.width;
		float mouseY = Input.mousePosition.y / (float)Screen.height;
		// Debug.Log($"Hello, sailor! {mouseX}, {mouseY}");
		
		LocalTransform transform = state.EntityManager.GetComponentData<LocalTransform>(playerEntity);
		transform.Position = transform.Position + new float3(moveX, 0, moveY) * SystemAPI.Time.DeltaTime * speed;
		
		state.EntityManager.SetComponentData<LocalTransform>(playerEntity, transform);
		
		if (Input.GetMouseButtonDown(0)) {
			// spawn a bullet!
			
			// maybe the game handle becomes invalid here once we spawn a new entity??? that'd be so dumb
			Entity bullet = state.EntityManager.Instantiate(game.ValueRO.BulletPrefab);
			
			BulletComponent bulletComponent = new BulletComponent();
			bulletComponent.Velocity = new float2(mouseX - 0.5f, mouseY - 0.5f) * 100f;
			
			LocalTransform spawnTransform = LocalTransform.FromPosition(transform.Position);
			
			state.EntityManager.SetComponentData<LocalTransform>(bullet, spawnTransform);
			
			// hmm... so adding a component is a structural change?
			state.EntityManager.AddComponent<BulletComponent>(bullet);
			state.EntityManager.SetComponentData<BulletComponent>(bullet, bulletComponent);
		}
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
		
		EntityQuery playersQuery = SystemAPI.QueryBuilder().WithAll<PlayerComponent>().Build();
		NativeArray<Entity> playerEntities = playersQuery.ToEntityArray(Allocator.Temp);

		// we want an array of player entities...
		// foreach (var (playerComponent, playerEntity) in SystemAPI.Query<RefRW<PlayerComponent>>().WithEntityAccess()) {
		foreach (var playerEntity in playerEntities) {
			
			
			// playerComponent.ValueRW.Velocity = new float2(1000f, 5000f);
			// LocalTransform transform = state.EntityManager.GetComponentData<LocalTransform>(playerEntity);
			
			UpdatePlayer(ref state, game, playerEntity);
		}
		
		UpdateBulletSystem(ref state);
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