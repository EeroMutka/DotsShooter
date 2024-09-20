using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;

[BurstCompile]
public partial struct CollisionSystem : ISystem
{
	[BurstCompile]
	public void OnCreate(ref SystemState state) {
	}
	
	[BurstCompile]
	public void OnDestroy(ref SystemState state) { }
	
	public struct CollisionDetectionTile
	{
		public NativeList<Entity> Entities;
	};

    [BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
        EntityManager em = state.EntityManager;
        EntityQuery entitiesQuery = SystemAPI.QueryBuilder().WithAll<ColliderComponent>().Build();
        NativeArray<Entity> entities = entitiesQuery.ToEntityArray(Allocator.Temp);

        const float worldLimits = 50f;

        int tileMapSize = 64;

        // Here we use a grid acceleration structure for finding colliding detection pairs to avoid an O(N^2) loop

        NativeArray<CollisionDetectionTile> entitiesInTile = new NativeArray<CollisionDetectionTile>(tileMapSize * tileMapSize, Allocator.Temp);
        for (int i = 0; i < entitiesInTile.Length; i++)
        {
            CollisionDetectionTile tile = new CollisionDetectionTile();
            tile.Entities = new NativeList<Entity>(Allocator.Temp);
            entitiesInTile[i] = tile;
        }

        // build a data structure
        foreach (Entity entity in entities)
        {
            LocalTransform transform = em.GetComponentData<LocalTransform>(entity);

            int tileX = Mathf.Clamp((int)(((transform.Position.x / worldLimits) * 0.5f + 0.5f) * tileMapSize), 0, tileMapSize - 1);
            int tileY = Mathf.Clamp((int)(((transform.Position.z / worldLimits) * 0.5f + 0.5f) * tileMapSize), 0, tileMapSize - 1);
            int tileIdx = tileY * tileMapSize + tileX;

            CollisionDetectionTile tile = entitiesInTile[tileIdx];
            tile.Entities.Add(entity);
            entitiesInTile[tileIdx] = tile;
        }

        for (int y = 0; y < tileMapSize; y++)
        {
            for (int x = 0; x < tileMapSize; x++)
            {
                CollisionDetectionTile tile = entitiesInTile[y * tileMapSize + x];

                foreach (Entity entity in tile.Entities)
                {
                    ColliderComponent colliderComponent = em.GetComponentData<ColliderComponent>(entity);
                    LocalTransform transform = em.GetComponentData<LocalTransform>(entity);

                    // Collision detect against other entities

                    for (int y2 = -1; y2 <= 1; y2++)
                    {
                        for (int x2 = -1; x2 <= 1; x2++)
                        {
                            int otherX = x - x2;
                            int otherY = y - y2;
                            if (otherX < 0 || otherX >= tileMapSize || otherY < 0 || otherY >= tileMapSize) continue;
                            CollisionDetectionTile otherTile = entitiesInTile[otherY * tileMapSize + otherX];

                            foreach (Entity otherEntity in otherTile.Entities)
                            {
                                if (otherEntity == entity) continue;

                                LocalTransform otherTransform = em.GetComponentData<LocalTransform>(otherEntity);

                                if (math.length(otherTransform.Position - transform.Position) < 1f)
                                {
                                    colliderComponent.LastCollidedWith = otherEntity;
                                }
                            }
                        }
                    }

                    em.SetComponentData<ColliderComponent>(entity, colliderComponent);
                }
            }
        }

    }
}