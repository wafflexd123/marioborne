using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIReplacerMB : MonoBehaviour
{
    [SerializeField] private List<GameObject> prefabs = new List<GameObject>();
    private Dictionary<string, GameObject> prefabsMap = new Dictionary<string, GameObject>();

    void Start()
    {
        
    }

    private void InitMap()
    {
        foreach (GameObject prefab in prefabs)
        {
            AIController controller = prefab.GetComponent<AIController>();
            switch (controller)
            {
                case StandardAI standard:

                    break;
            }
        }
    }

    public void ReplaceAllInScene()
    {

    }

    public void ReplaceSingle()
    {

    }
}
