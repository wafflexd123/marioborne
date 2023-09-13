using UnityEngine;
using UnityEngine.SceneManagement;

public class UnityEventHelper : MonoBehaviourPlus
{
	new public void Destroy(Object obj)
	{
		Object.Destroy(obj);
	}

	public void SetPosition(Transform t)
	{
		transform.position = t.position;
	}

	public void SetRotation(Transform t)
	{
		transform.rotation = t.rotation;
	}

    public void DebugLog(string message)
    {
        Debug.Log(message, this);
    }

    public void LoadScene(string scene)
    {
        SceneManager.LoadScene(scene, LoadSceneMode.Single);
    }

	public void RemoveParent(Transform transform)
	{
		transform.SetParent(null);
	}

	public void SetTimeScale(float time)
	{
		Time.timeScale = time;
	}
}
