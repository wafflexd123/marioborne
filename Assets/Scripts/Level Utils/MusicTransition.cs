using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicTransition : MonoBehaviour
{
    private AudioSource audioSource;
    private bool fadeOut;
    private float time;
    public float fadeToVolume = 0.25f;
    public float fadeFactor;
    [HideInInspector] public AudioClip clip;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (fadeOut) //fade out by [fadeFactor] per second
        {
            if(audioSource.volume > fadeToVolume + 0.01f)
                audioSource.volume -= fadeFactor * Time.fixedDeltaTime;
            else
            {
                time = audioSource.time;
                audioSource.clip = clip;
                audioSource.time = time;
                audioSource.Play();
                fadeOut = false;
            }
        }

        if (!fadeOut) //fade in by [fadeFactor] per second
        {
            if (audioSource.volume < 1f)
                audioSource.volume += fadeFactor * Time.fixedDeltaTime;
        }
    }

    public void Transition(AudioClip song)
    {
        clip = song;
        fadeOut = true;
    }
}
