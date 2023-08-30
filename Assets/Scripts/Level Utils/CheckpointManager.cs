using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager instance;
    public Vector3 lastCheckpointPos;
    public int lastCheckpoint;
    int lastScene = -1;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.activeSceneChanged += (Scene a, Scene b) =>
            {
                if (lastScene == b.buildIndex)
                {
                    if (Player.singlePlayer != null)
                    {
                        Player.singlePlayer.transform.position = lastCheckpointPos;//if reloading the same scene, spawn at checkpoint
                        StartCoroutine(IHateSocietyAndUnityAndEverythingSucks(lastCheckpointPos));
                    }
                }
                else
                {
                    lastCheckpoint = 0;
                    lastCheckpointPos = Player.singlePlayer != null ? Player.singlePlayer.transform.position : Vector3.zero;//player will respawn where it starts in the level, until hitting a checkpoint
                    lastScene = b.buildIndex;//if reloading to a different scene, store new scene
                }
            };
        }
        else
        {
            Destroy(gameObject);
        }
    }

    IEnumerator IHateSocietyAndUnityAndEverythingSucks(Vector3 pos)
    {
        yield return new WaitForFixedUpdate();
        Player.singlePlayer.transform.position = pos;//bruh
    }
}

