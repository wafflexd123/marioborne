using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DreamLevelManager : MonoBehaviourPlus
{
	public RawImage whiteFade;
	public float startTime, whiteFadeTime;
	public GameObject firstSection;
	IEnumerator Start()
	{
		float timer;
		whiteFade.gameObject.SetActive(true);
		for (timer = 0; timer < startTime; timer += Time.deltaTime) yield return null;
		for (timer = 0; timer < whiteFadeTime; timer += Time.deltaTime)
		{
			whiteFade.color = new Color(1, 1, 1, 1 - (timer / whiteFadeTime));
			yield return null;
		}
		Destroy(whiteFade.gameObject);
		yield return new WaitUntil(() => firstSection == null);
	}
}
