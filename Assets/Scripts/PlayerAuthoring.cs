using UnityEngine;
using System.Collections;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;

class PlayerAuthoring : MonoBehaviour
{
}

class PlayerBaker : Baker<PlayerAuthoring>
{
	public override void Bake(PlayerAuthoring authoring)
	{
		var entity = GetEntity(TransformUsageFlags.None);
		
		// we want a local-transform component right??
		AddComponent(entity, new LocalTransform{
			Position = authoring.gameObject.transform.position,
		});
		
		AddComponent(entity, new PlayerComponent
		{
		});
	}
}


/*
What I want:
After dinner, you tell me: "come here!" and I join you in your blanket.

Then we talk a bit about random things or your things or my things and maybe watch some tiktok or something. Or do nothing and just cuddle.

Then if we don't have any plans or date night, one of us is like "what are you thinking of doing today?" Then the other answers: "I just feel like doing some work" or "playing a game" or "I actually would really like to do this X thing together! Would you be up for that?"

Then we do something together or on our own. If we do things on our own, then maybe in the middle of it me or you could come check on the other and say hello and maybe hug and maybe have a quick chat.


*/