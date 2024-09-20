using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;

[BurstCompile]
public partial struct BulletSystem : ISystem
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
        if (transform.Position.x > worldLimits || transform.Position.x < -worldLimits ||
            transform.Position.z > worldLimits || transform.Position.z < -worldLimits)
        {
            em.DestroyEntity(entity);
        }

    }

    [BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
        EntityQuery bulletsQuery = SystemAPI.QueryBuilder().WithAll<BulletComponent>().Build();
        NativeArray<Entity> bulletEntities = bulletsQuery.ToEntityArray(Allocator.Temp);

        foreach (Entity bulletEntity in bulletEntities)
        {
            UpdateBullet(ref state, bulletEntity);
        }
    }
}
