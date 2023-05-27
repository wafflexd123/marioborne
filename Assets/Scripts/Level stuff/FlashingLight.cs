using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashingLight : MonoBehaviour
{
	public float flashTime;
	public new Light light;
	float timer = 0;

	void FixedUpdate()
	{
		timer += Time.fixedDeltaTime;
		if (timer >= flashTime)
		{
			timer = 0;
			light.enabled = !light.enabled;
		}
	}
}
