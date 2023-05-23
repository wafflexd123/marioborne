using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitRestart : MonoBehaviour
{
	[SerializeField] private int currentSceneIndex;

	public KeyCode restartKey = KeyCode.R;
	public KeyCode exitKey = KeyCode.Escape;

	private void Start()
	{
		DontDestroyOnLoad(this);
		Debug.Log("Loaded (should only see this once)");
		SceneManager.activeSceneChanged += (Scene a, Scene b) => Cursor.lockState = CursorLockMode.None;
	}

	private void Update()
	{
		currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

		if (Input.GetKeyDown(restartKey))
		{
			SceneManager.LoadScene(currentSceneIndex);
		}

		if (Input.GetKeyDown(exitKey))
		{
			if (currentSceneIndex != 0)
			{
				SceneManager.LoadScene(0);
				Cursor.lockState = CursorLockMode.None;
			}
			else
			{
				Debug.Log("Game Quit.");
				Application.Quit();
			}
		}
	}

	public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		Debug.Log("Teleporting Player");
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (CheckpointManager.instance != null && player != null)
        {
            player.transform.position = CheckpointManager.instance.lastCheckpointPos;
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
