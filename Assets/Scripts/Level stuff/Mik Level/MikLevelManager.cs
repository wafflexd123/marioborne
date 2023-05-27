using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MikLevelManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
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
