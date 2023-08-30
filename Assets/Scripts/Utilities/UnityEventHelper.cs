using UnityEngine;
using UnityEngine.SceneManagement;

public class UnityEventHelper : MonoBehaviour
{
    public void TriggerDelete(MonoBehaviour monoBehaviour)
    {
        Destroy(monoBehaviour);
    }

    public void TriggerDelete(GameObject gameObject)
    {
        Destroy(gameObject);
    }

    public void SetParent(Transform transform)
    {
        this.transform.SetParent(transform);
    }

    public void DebugLog(string message)
    {
        Debug.Log(message, this);
    }

    public void LoadScene(string scene)
    {
        SceneManager.LoadScene(scene, LoadSceneMode.Single);
    }
}
