using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MikLevelManager : MonoBehaviour
{

    public GameObject Enemies1, Enemies2, Enemies3, Building2, Building25, Building3, Building4, Building5, BgBuildings;
    // Start is called before the first frame update
    void Start()
    {
        switch (CheckpointManager.instance.lastCheckpoint)
        {
            case 1:
                Destroy(Enemies1);
                Destroy(Enemies2);
                Destroy(Building2);
                break;

            case 2:
                Destroy(Building2);
                Destroy(Building25);
                Destroy(Building3);
                Destroy(Building4);
                Destroy(Building5);
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoadNextLevel()
    {
        SceneManager.LoadSceneAsync("Descent V2", LoadSceneMode.Single);
    }
}
