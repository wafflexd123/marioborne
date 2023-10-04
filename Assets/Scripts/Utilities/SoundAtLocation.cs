using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundAtLocation : MonoBehaviour
{
    [SerializeField] private int instances = 5;
    private int index = 0;
    private GameObject[] objs;
    private AudioSource[] aus;

    public static SoundAtLocation Instance;

    void Start()
    {
        Instance = this;

        objs = new GameObject[instances];
        aus = new AudioSource[instances];
        for (int i=0;i<instances; i++)
        {
            GameObject guy = new GameObject("Sound guy " + i);
            guy.transform.parent = transform;
            AudioSource a = guy.AddComponent<AudioSource>();
            //a.spatialBlend = 1f;
            objs[i] = guy;
            aus[i] = a;
        }
    }

    public void PlayAtLocation(Vector3 pos, AudioClip clip)
    {
        objs[index].transform.position = pos;
        aus[index].clip = clip;
        aus[index].Play();
        index = (index + 1) % instances;
    }
}
