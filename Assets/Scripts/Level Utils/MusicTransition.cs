using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicTransition : MonoBehaviour
{
    private AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Transition(AudioClip song)
    {
        float time = audioSource.time;
        audioSource.clip = song;
        audioSource.time = time;
        audioSource.Play();
    }
}
