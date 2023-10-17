using UnityEngine;
using UnityEngine.SceneManagement;

public class UnityEventHelper : MonoBehaviourPlus
{
	new public void Destroy(Object obj)
	{
		Object.Destroy(obj);
	}

	public void SetTransform(Transform t)
	{
		transform.position = t.position;
		transform.rotation = t.rotation;
		transform.localScale = t.localScale;
	}

	public void SetPosition(Transform t)
	{
		transform.position = t.position;
	}

	public void SetRotation(Transform t)
	{
		transform.rotation = t.rotation;
	}

	public void SetScale(Transform t)
	{
		transform.localScale = t.localScale;
	}

    public void DebugLog(string message)
    {
        Debug.Log(message, this);
    }

	public void RemoveParent(Transform transform)
	{
		transform.SetParent(null);
	}

	public void SetTimeScale(float time)
	{
		Time.timeScale = time;
	}

	public void LoadScene(string scene)
	{
		Time.timeScale = 1;
		SceneManager.LoadScene(scene, LoadSceneMode.Single);
	}

	public void ReloadCurrentScene()
	{
		Time.timeScale = 1;
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	public void ExitGame()
	{
		Application.Quit();
	}
}
