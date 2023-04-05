using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnUIControl : MonoBehaviour
{
    void Update()
    {
		if (Input.GetKeyDown(KeyCode.Space))
		{
			SceneLoadTools.singleton.ReloadCurrentScene();
		}
    }
}
