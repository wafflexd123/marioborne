using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DreamLevelManager : MonoBehaviourPlus
{
	public RawImage whiteFade;
	public float startTime, whiteFadeTime;
	public string nextScene;
	public GameObject player;
	public bool switchScene;

	IEnumerator Start()
	{
		whiteFade.gameObject.SetActive(true);
		yield return new WaitForSeconds(startTime);
		for (float timer = 0; timer < whiteFadeTime; timer += Time.deltaTime)
		{
			whiteFade.color = new Color(1, 1, 1, 1 - (timer / whiteFadeTime));
			yield return null;
		}
		Destroy(whiteFade.gameObject);
	}
}
