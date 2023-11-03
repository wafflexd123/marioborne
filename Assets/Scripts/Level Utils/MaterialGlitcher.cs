using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialGlitcher : MonoBehaviour
{
	public float minTimeBetweenChecks, maxTimeBetweenChecks, minGlitchTime, maxGlitchTime;
	[Range(0f, 1f)] public float glitchChance;
	public Material glitchMaterial;

	private void OnEnable()
	{
		StartCoroutine(Glitch());
	}

	IEnumerator Glitch()
	{
		while (true)
		{
			if (Random.Range(0f, 1f) <= glitchChance)
			{
				if (TryGetComponent(out Renderer r))
				{
					Material temp = r.material;
					r.material = glitchMaterial;
					yield return new WaitForSeconds(Random.Range(minGlitchTime, maxGlitchTime));
					r.material = temp;
				}
				else Debug.LogWarning("MaterialGlitcher must be attached to a renderer!", this);
			}
			yield return new WaitForSeconds(Random.Range(minTimeBetweenChecks, maxTimeBetweenChecks));
		}
	}
}
