using UnityEngine;
using System.Collections;
using System.Collections.Generic;
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
	public void UpdateBullet(ref SystemState state, Entity entity)
	{
		EntityManager em = state.EntityManager;
		BulletComponent bulletComponent = em.GetComponentData<BulletComponent>(entity);
		ColliderComponent colliderComponent = em.GetComponentData<ColliderComponent>(entity);
		LocalTransform transform = em.GetComponentData<LocalTransform>(entity);
		
		float2 velocity = bulletComponent.Velocity;
		
		transform.Position += new float3(velocity.x, 0, velocity.y) * SystemAPI.Time.DeltaTime;
		
		em.SetComponentData<LocalTransform>(entity, transform);
		
		const float worldLimits = 50f;
		
		// Destroy bullet if out of bounds
		if (colliderComponent.DidCollide ||
			transform.Position.x > worldLimits || transform.Position.x < -worldLimits ||
			transform.Position.z > worldLimits || transform.Position.z < -worldLimits)
		{
			em.DestroyEntity(entity);
		}
		
	}
	
	[BurstCompile]
	public void UpdateBulletSystem(ref SystemState state) {
		EntityQuery bulletsQuery = SystemAPI.QueryBuilder().WithAll<BulletComponent>().Build();
		NativeArray<Entity> bulletEntities = bulletsQuery.ToEntityArray(Allocator.Temp);
		
		foreach (Entity bulletEntity in bulletEntities) {
			UpdateBullet(ref state, bulletEntity);
		}
	}
	
	[BurstCompile]
	public void UpdateEnemySystem(ref SystemState state)
	{
		EntityQuery bulletsQuery = SystemAPI.QueryBuilder().WithAll<EnemyComponent>().Build();
		NativeArray<Entity> enemyEntities = bulletsQuery.ToEntityArray(Allocator.Temp);
		
		foreach (Entity enemyEntity in enemyEntities) {
			UpdateEnemy(ref state, enemyEntity);
		}
	}
	
	public struct CollisionDetectionTile
	{
		public NativeList<Entity> Entities;
	};
	
	
	[BurstCompile]
	public void UpdateCollisionDetectionSystem(ref SystemState state)
	{
		EntityManager em = state.EntityManager;
		EntityQuery entitiesQuery = SystemAPI.QueryBuilder().WithAll<ColliderComponent>().Build();
		NativeArray<Entity> entities = entitiesQuery.ToEntityArray(Allocator.Temp);
		
		const float worldLimits = 50f;
		
		// do collision detection using a grid...
		// int[] entitiesInTile = new int[50*50];
		
		int tileMapSize = 64;
		
		NativeArray<CollisionDetectionTile> entitiesInTile = new NativeArray<CollisionDetectionTile>(tileMapSize*tileMapSize, Allocator.Temp);
		for (int i = 0; i < entitiesInTile.Length; i++) {
			CollisionDetectionTile tile = new CollisionDetectionTile();
			tile.Entities = new NativeList<Entity>(Allocator.Temp);
			entitiesInTile[i] = tile;
		}
		
		// build a data structure
		foreach (Entity entity in entities) {
			LocalTransform transform = em.GetComponentData<LocalTransform>(entity);
			
			int tileX = Mathf.Clamp((int)(((transform.Position.x / worldLimits) * 0.5f + 0.5f) * tileMapSize), 0, tileMapSize - 1);
			int tileY = Mathf.Clamp((int)(((transform.Position.z / worldLimits) * 0.5f + 0.5f) * tileMapSize), 0, tileMapSize - 1);
			int tileIdx = tileY*tileMapSize + tileX;
			
			CollisionDetectionTile tile = entitiesInTile[tileIdx];
			tile.Entities.Add(entity);
			entitiesInTile[tileIdx] = tile;
		}
		
		for (int y = 0; y < tileMapSize; y++) {
			for (int x = 0; x < tileMapSize; x++) {
				CollisionDetectionTile tile = entitiesInTile[y*tileMapSize + x];
				
				foreach (Entity entity in tile.Entities) {
					ColliderComponent colliderComponent = em.GetComponentData<ColliderComponent>(entity);
					LocalTransform transform = em.GetComponentData<LocalTransform>(entity);
					
					// Collision detect against other entities
					
					for (int y2 = -1; y2 <= 1; y2++) {
						for (int x2 = -1; x2 <= 1; x2++) {
							int otherX = x - x2;
							int otherY = y - y2;
							if (otherX < 0 || otherX >= tileMapSize || otherY < 0 || otherY >= tileMapSize) continue;
							CollisionDetectionTile otherTile = entitiesInTile[otherY*tileMapSize + otherX];
							
							foreach (Entity otherEntity in otherTile.Entities) {
								if (otherEntity == entity) continue;
								
								LocalTransform otherTransform = em.GetComponentData<LocalTransform>(otherEntity);
								
								if (math.length(otherTransform.Position - transform.Position) < 1f) {
									colliderComponent.DidCollide = true;
								}
							}
						}
					}
					
					em.SetComponentData<ColliderComponent>(entity, colliderComponent);
				}
			}
		}
	}
	
	[BurstCompile]
	public void UpdateEnemy(ref SystemState state, Entity enemy)
	{
		EntityManager em = state.EntityManager;
		EnemyComponent enemyComponent = em.GetComponentData<EnemyComponent>(enemy);
		
		LocalTransform transform = em.GetComponentData<LocalTransform>(enemy);
		
		EntityQuery playersQuery = SystemAPI.QueryBuilder().WithAll<PlayerComponent>().Build();
		NativeArray<Entity> players = playersQuery.ToEntityArray(Allocator.Temp);
		
		// Move towards the player
		if (players.Length > 0) {
			Entity player = players[0];
			LocalTransform playerTransform = em.GetComponentData<LocalTransform>(player);
			
			transform.Position += math.normalize(playerTransform.Position - transform.Position) * SystemAPI.Time.DeltaTime * 10f;
		}
		
		em.SetComponentData<LocalTransform>(enemy, transform);
		state.EntityManager.SetComponentData<EnemyComponent>(enemy, enemyComponent);
	}
	
	[BurstCompile]
	public void UpdatePlayer(ref SystemState state, GameComponent game, Entity playerEntity)
	{
		EntityManager em = state.EntityManager;
		PlayerComponent playerComponent = em.GetComponentData<PlayerComponent>(playerEntity);
		
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
		
		const float speed = 20f;
		const float bulletSpawnInterval = 0.05f;
		
		// let's just use an orthographic camera.
		float mouseX = Input.mousePosition.x / (float)Screen.width;
		float mouseY = Input.mousePosition.y / (float)Screen.height;
		// Debug.Log($"Hello, sailor! {mouseX}, {mouseY}");
		
		LocalTransform transform = em.GetComponentData<LocalTransform>(playerEntity);
		transform.Position = transform.Position + new float3(moveX, 0, moveY) * SystemAPI.Time.DeltaTime * speed;
		
		em.SetComponentData<LocalTransform>(playerEntity, transform);
		
		if (Input.GetMouseButtonDown(0)) { // reset shoot timer
			playerComponent.NextBulletSpawnTimer = 0f;
		}
		
		if (Input.GetMouseButton(0)) {
			playerComponent.NextBulletSpawnTimer -= SystemAPI.Time.DeltaTime;

			if (playerComponent.NextBulletSpawnTimer < 0) {
				playerComponent.NextBulletSpawnTimer = bulletSpawnInterval;
			
				// Spawn a new bullet
				
				Entity bullet = em.Instantiate(game.BulletPrefab);
				em.AddComponent<ColliderComponent>(bullet);
				em.AddComponent<BulletComponent>(bullet);
				
				BulletComponent bulletComponent = new BulletComponent();
				bulletComponent.Velocity = math.normalize(new float2(mouseX - 0.5f, mouseY - 0.5f)) * 50f;
				
				em.SetComponentData<LocalTransform>(bullet, LocalTransform.FromPosition(transform.Position));
				
				// hmm... so adding a component is a structural change?
				em.SetComponentData<BulletComponent>(bullet, bulletComponent);
			}
		}
		
		// Update component data
		em.SetComponentData<PlayerComponent>(playerEntity, playerComponent);
	}
	
	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		EntityManager em = state.EntityManager;
		
		EntityQuery gamesQuery = SystemAPI.QueryBuilder().WithAll<GameComponent>().Build();
		Entity gameEntity = gamesQuery.ToEntityArray(Allocator.Temp)[0];
		GameComponent game = em.GetComponentData<GameComponent>(gameEntity);
		
		if (game.FrameIndex == 0) { // Instantiating entities doesn't seem to work inside OnCreate, so spawn during the first frame instead
			Entity playerEntity = em.Instantiate(game.PlayerPrefab);
			em.AddComponent<PlayerComponent>(playerEntity);
			// em.AddComponent<ColliderComponent>(playerEntity);
			
			LocalTransform playerTransform = LocalTransform.FromPosition(new float3(0, 0, 0));
			em.SetComponentData<LocalTransform>(playerEntity, playerTransform);
		}
		game.FrameIndex += 1;
		
		// foreach (RefRW<PlayerComponent> playerComponent in SystemAPI.Query<RefRW<PlayerComponent>>()) {
		
		EntityQuery playersQuery = SystemAPI.QueryBuilder().WithAll<PlayerComponent>().Build();
		NativeArray<Entity> playerEntities = playersQuery.ToEntityArray(Allocator.Temp);

		// we want an array of player entities...
		// foreach (var (playerComponent, playerEntity) in SystemAPI.Query<RefRW<PlayerComponent>>().WithEntityAccess()) {
		foreach (Entity playerEntity in playerEntities) {
			// playerComponent.ValueRW.Velocity = new float2(1000f, 5000f);
			// LocalTransform transform = em.GetComponentData<LocalTransform>(playerEntity);
			
			UpdatePlayer(ref state, game, playerEntity);
		}
		
		// Spawn enemies
		game.NextEnemySpawnTime -= SystemAPI.Time.DeltaTime;
		if (game.NextEnemySpawnTime < 0f) {
			game.NextEnemySpawnTime = 2f;
			
			Entity enemy = em.Instantiate(game.EnemyPrefab);
			em.AddComponent<EnemyComponent>(enemy);
			em.AddComponent<ColliderComponent>(enemy);
			
			LocalTransform playerTransform = LocalTransform.FromPosition(new float3(UnityEngine.Random.Range(-50f, 50f), 0, UnityEngine.Random.Range(-50f, 50f)));
			em.SetComponentData<LocalTransform>(enemy, playerTransform);
		}
		
		UpdateCollisionDetectionSystem(ref state);
		UpdateBulletSystem(ref state);
		UpdateEnemySystem(ref state);
		
		em.SetComponentData<GameComponent>(gameEntity, game);
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