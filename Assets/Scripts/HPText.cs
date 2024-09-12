using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using TMPro;

public class HPText : MonoBehaviour
{
	public TextMeshProUGUI Text;
	
	void Update()
	{
		EntityManager em = World.DefaultGameObjectInjectionWorld.EntityManager;
		
		var query = em.CreateEntityQuery(typeof(PlayerComponent));
		NativeArray<Entity> entities = query.ToEntityArray(Allocator.Temp);
		
		if (entities.Length == 1)
		{
			PlayerComponent player = em.GetComponentData<PlayerComponent>(entities[0]);
			
			Text.text = $"HP: {player.Health}";
		}
		
	}
}
