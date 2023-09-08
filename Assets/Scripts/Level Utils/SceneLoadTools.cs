using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadTools : MonoBehaviour
{
    public static SceneLoadTools singleton;

    private void Awake()
    {
        singleton = this;
    }

    public void LoadScene(string scene)
    {
        SceneManager.LoadScene(scene);
    }

    public void LoadScene(int scene)
    {
        SceneManager.LoadScene(scene);
    }

    public void ReloadCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
