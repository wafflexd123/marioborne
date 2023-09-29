using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollManager : MonoBehaviour
{
    public Rigidbody mainRigidbody;
    private Rigidbody[] ragdollRigidbodies;
    private Collider[] ragdollColliders;
    private Animator enemyAnimator;
    [SerializeField] private MonoBehaviour[] componentsToDisable;

    private void Start()
    {
        ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
        ragdollColliders = GetComponentsInChildren<Collider>();
        enemyAnimator = GetComponent<Animator>();

        ToggleRagdoll(false);
    }

    public void ToggleRagdoll(bool state)
    {
        enemyAnimator.enabled = !state;

        foreach (MonoBehaviour monoBehaviour in componentsToDisable)
        {
            monoBehaviour.enabled = !state;
        }
        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            rb.isKinematic = !state;
        }
        foreach (Collider collider in ragdollColliders)
        {
            collider.enabled = state;
        }

        if (state)
        {
            mainRigidbody.isKinematic = true;
        }
        else
        {
            mainRigidbody.isKinematic = false;
            if (ragdollRigidbodies.Length > 0)
            {
                mainRigidbody.velocity = ragdollRigidbodies[0].velocity;
            }
        }
    }
}

