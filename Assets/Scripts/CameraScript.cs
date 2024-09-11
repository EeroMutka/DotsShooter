using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
public class CameraScript : MonoBehaviour
{
	// Start is called before the first frame update
	void Start()
	{
		
	}

	// Update is called once per frame
	void Update()
	{
		EntityManager em = World.DefaultGameObjectInjectionWorld.EntityManager;
		
		var query = em.CreateEntityQuery(typeof(PlayerComponent));
		NativeArray<Entity> entities = query.ToEntityArray(Allocator.Temp);
		
		if (entities.Length == 1)
		{
			Entity entity = entities[0];
			LocalTransform playerTransform = em.GetComponentData<LocalTransform>(entity);
			
			Vector3 targetPosition =
				new Vector3(playerTransform.Position.x, playerTransform.Position.y, playerTransform.Position.z) +
				new Vector3(0, 10f, 0);
				
			transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 10f);
		}
		
	}
}
