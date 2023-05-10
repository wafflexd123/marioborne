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
                Cursor.lockState = CursorLockMode.Confined;
            }
            else
            {
                Debug.Log("Game Quit.");
                Application.Quit();
            }
        }
    }
}
