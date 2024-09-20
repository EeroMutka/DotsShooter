using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;

[BurstCompile]
public partial struct EnemySystem : ISystem
{
	[BurstCompile]
	public void OnCreate(ref SystemState state) {
	}
	
	[BurstCompile]
	public void OnDestroy(ref SystemState state) { }
	
	[BurstCompile]
	public void UpdateEnemy(ref SystemState state, Entity enemy)
	{
		EntityManager em = state.EntityManager;
		EnemyComponent enemyComponent = em.GetComponentData<EnemyComponent>(enemy);
		ColliderComponent collider = em.GetComponentData<ColliderComponent>(enemy);
		
		LocalTransform transform = em.GetComponentData<LocalTransform>(enemy);
		
		EntityQuery playersQuery = SystemAPI.QueryBuilder().WithAll<PlayerComponent>().Build();
		NativeArray<Entity> players = playersQuery.ToEntityArray(Allocator.Temp);
		
		// Move towards the player
		if (players.Length > 0) {
			Entity player = players[0];
			LocalTransform playerTransform = em.GetComponentData<LocalTransform>(player);

			float3 forward = math.normalize(playerTransform.Position - transform.Position);
			float3 right = new float3(-forward.z, 0, forward.x);

            const float speed = 10f;
            const float wiggle = 1f;
            const float wiggleFrequency = 5f;
			float3 wiggleVector = right * wiggle * (float)math.sin(wiggleFrequency * SystemAPI.Time.ElapsedTime + (float)enemy.Index);

            transform.Position += (forward + wiggleVector) * SystemAPI.Time.DeltaTime * speed;

		}
		
		em.SetComponentData<LocalTransform>(enemy, transform);
		state.EntityManager.SetComponentData<EnemyComponent>(enemy, enemyComponent);
		
		if (collider.LastCollidedWith != Entity.Null) {
			em.DestroyEntity(enemy);
		}
	}
	
    [BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
        EntityQuery bulletsQuery = SystemAPI.QueryBuilder().WithAll<EnemyComponent>().Build();
        NativeArray<Entity> enemyEntities = bulletsQuery.ToEntityArray(Allocator.Temp);

        foreach (Entity enemyEntity in enemyEntities)
        {
            UpdateEnemy(ref state, enemyEntity);
        }
    }
}