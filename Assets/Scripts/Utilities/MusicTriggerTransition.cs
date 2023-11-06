using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicTriggerTransition : MonoBehaviour
{
    public AudioClip songToTransitionTo;
    [HideInInspector] public Collider other;
    [HideInInspector] public bool isTriggered;
    private MusicTransition musicTransition;

    private void Awake()
    {
        Collider collider = GetComponent<Collider>();
        collider.isTrigger = true;
        if (collider.excludeLayers == 0 || collider.includeLayers == 0) Debug.LogWarning("Trigger collider might not have correct include/exclude layers; might get unintentionally triggered", this);
        GameObject BGM = GameObject.FindGameObjectWithTag("Music");
        musicTransition = BGM.GetComponent<MusicTransition>();
    }

    private void OnTriggerEnter(Collider other)
    {
        musicTransition.Transition(songToTransitionTo);
    }
}
