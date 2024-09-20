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
	public void OnUpdate(ref SystemState state)
	{
		EntityManager em = state.EntityManager;
		
		EntityQuery gamesQuery = SystemAPI.QueryBuilder().WithAll<GameComponent>().Build();
		NativeArray<Entity> gameEntities = gamesQuery.ToEntityArray(Allocator.Temp);
		
		if (gameEntities.Length > 0)
		{
			GameComponent game = em.GetComponentData<GameComponent>(gameEntities[0]);
            if (game.FrameIndex == 0)
            {
				// Spawn player! Instantiating entities doesn't seem to work inside OnCreate, so do it during the first frame
                Entity playerEntity = em.Instantiate(game.PlayerPrefab);
                em.AddComponent<PlayerComponent>(playerEntity);
                em.AddComponent<ColliderComponent>(playerEntity);

                PlayerComponent playerComponent = new PlayerComponent { Health = 50 };
                LocalTransform playerTransform = LocalTransform.FromPosition(new float3(0, 0, 0));

                em.SetComponentData<LocalTransform>(playerEntity, playerTransform);
                em.SetComponentData<PlayerComponent>(playerEntity, playerComponent);
            }

			bool waveIsActive = math.frac(SystemAPI.Time.ElapsedTime / 10f) < 0.3f;
			if (waveIsActive)
			{
				// Spawn enemies
				game.NextEnemySpawnTime -= SystemAPI.Time.DeltaTime;
				if (game.NextEnemySpawnTime < 0f) {
					game.NextEnemySpawnTime = 0.07f;

					Entity enemy = em.Instantiate(game.EnemyPrefab);
					em.AddComponent<EnemyComponent>(enemy);
					em.AddComponent<ColliderComponent>(enemy);

					float3 enemyPos = math.normalize(new float3(UnityEngine.Random.Range(-1f, 1f), 0, UnityEngine.Random.Range(-1f, 1f))) * 80f;
                    LocalTransform enemyTransform = LocalTransform.FromPosition(enemyPos);
					em.SetComponentData<LocalTransform>(enemy, enemyTransform);
				}
			}

            game.FrameIndex += 1;
			em.SetComponentData<GameComponent>(gameEntities[0], game);
        }
    }
}