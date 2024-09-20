using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;

[BurstCompile]
public partial struct PlayerSystem : ISystem
{
	[BurstCompile]
	public void OnCreate(ref SystemState state) {
	}
	
	[BurstCompile]
	public void OnDestroy(ref SystemState state) { }
	
	[BurstCompile]
	public void UpdatePlayer(ref SystemState state, GameComponent game, Entity playerEntity)
	{
		EntityManager em = state.EntityManager;
		PlayerComponent playerComponent = em.GetComponentData<PlayerComponent>(playerEntity);
		ColliderComponent collider = em.GetComponentData<ColliderComponent>(playerEntity);

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
		
		LocalTransform transform = em.GetComponentData<LocalTransform>(playerEntity);
		transform.Position = transform.Position + new float3(moveX, 0, moveY) * SystemAPI.Time.DeltaTime * speed;
		transform.Position = new float3(Mathf.Clamp(transform.Position.x, -50f, 50f), transform.Position.y, Mathf.Clamp(transform.Position.z, -50f, 50f));
		
		em.SetComponentData<LocalTransform>(playerEntity, transform);
		
		if (Input.GetMouseButtonDown(0)) { // reset shoot timer
			playerComponent.NextBulletSpawnTimer = 0f;
		}

		if (em.HasComponent<EnemyComponent>(collider.LastCollidedWith))
        {
			// collided with enemy!
            playerComponent.Health -= 1;
			collider.LastCollidedWith = Entity.Null;
		}
		
		if (Input.GetMouseButton(0)) {
			playerComponent.NextBulletSpawnTimer -= SystemAPI.Time.DeltaTime;

			if (playerComponent.NextBulletSpawnTimer < 0) {
				playerComponent.NextBulletSpawnTimer = bulletSpawnInterval;
			
				// Spawn a bullet
				
				Entity bullet = em.Instantiate(game.BulletPrefab);
				em.AddComponent<ColliderComponent>(bullet);
				em.AddComponent<BulletComponent>(bullet);
				
				BulletComponent bulletComponent = new BulletComponent {
					Velocity = math.normalize(new float2(mouseX - 0.5f, mouseY - 0.5f)) * 50f,
				};
				
				em.SetComponentData<LocalTransform>(bullet, LocalTransform.FromPosition(transform.Position));
				em.SetComponentData<BulletComponent>(bullet, bulletComponent);
			}
		}
		
		// Update component data
		em.SetComponentData<PlayerComponent>(playerEntity, playerComponent);
        em.SetComponentData<ColliderComponent>(playerEntity, collider);

    }

    [BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		EntityManager em = state.EntityManager;
		
		EntityQuery gamesQuery = SystemAPI.QueryBuilder().WithAll<GameComponent>().Build();
		NativeArray<Entity> gameEntities = gamesQuery.ToEntityArray(Allocator.Temp);

		if (gameEntities.Length > 0)
		{
			GameComponent game = em.GetComponentData<GameComponent>(gameEntities[0]);
			EntityQuery playersQuery = SystemAPI.QueryBuilder().WithAll<PlayerComponent>().Build();
			NativeArray<Entity> playerEntities = playersQuery.ToEntityArray(Allocator.Temp);

			foreach (Entity playerEntity in playerEntities)
			{
				UpdatePlayer(ref state, game, playerEntity);
			}
		}
    }
}