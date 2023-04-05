using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnOnPlayerTouch : MonoBehaviourPlus
{
	private void OnCollisionEnter(Collision collision)
	{
		if (FindComponent(collision.transform, out Player _))
		{
			SceneLoadTools.singleton.ReloadCurrentScene();
		}
	}
}
