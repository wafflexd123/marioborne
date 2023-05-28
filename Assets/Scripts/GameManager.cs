using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour //might use this later lol - I'm commandeering this code - Pluuskie
{
	public static GameManager singleton;
	[HideInInspector] public float savedSensitivity = 1;

	private void Awake()
	{
		if (singleton == null)
		{
			singleton = this;
			DontDestroyOnLoad(gameObject);
			//savedSensitivity = Player.singlePlayer.transform.Find("Head").GetComponent<PlayerCamera>().Sensitivity;
			SceneManager.activeSceneChanged += (Scene a, Scene b) =>
			{
				if (Player.singlePlayer != null) Player.singlePlayer.transform.Find("Head").GetComponent<PlayerCamera>().Sensitivity = savedSensitivity;
			};
		}
		else
		{
			Destroy(gameObject);//don't let more than one of these exist
		}
	}
}

