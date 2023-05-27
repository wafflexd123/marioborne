using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckpointManager : MonoBehaviour
{
	public static CheckpointManager instance;
	public Vector3 lastCheckpointPos;
	public int lastCheckpoint;
	int lastScene;

	void Awake()
	{
		if (instance == null)
		{
			instance = this;
			lastScene = SceneManager.GetActiveScene().buildIndex;
			DontDestroyOnLoad(gameObject);
			SceneManager.activeSceneChanged += (Scene a, Scene b) =>
			{
				if (lastScene == b.buildIndex)
				{
					Player.singlePlayer.transform.position = lastCheckpointPos;//if reloading the same scene, spawn at checkpoint
				}
				else
				{
					lastCheckpoint = 0;
					lastScene = b.buildIndex;//if reloading to a different scene, store new scene
				}
			};
		}
		else
		{
			Destroy(gameObject);
		}
	}
}

