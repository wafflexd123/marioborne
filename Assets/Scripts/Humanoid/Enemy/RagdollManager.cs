using System.Linq;
using UnityEngine;

public class RagdollManager : MonoBehaviour
{
    [SerializeField] private Transform hipsLocation;
    private Animator animator;
    private Rigidbody mainRigidbody;
    private Collider mainCollider;
    private Rigidbody[] ragdollRigidbodies;
    private Collider[] ragdollColliders;
    [SerializeField] private MonoBehaviour[] scriptsToDisable;

    private bool isRagdolling;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        mainRigidbody = GetComponent<Rigidbody>();
        mainCollider = GetComponentsInChildren<Collider>(true).FirstOrDefault(col => col.gameObject.tag == "MainCollider");

        ragdollRigidbodies = GetComponentsInChildren<Rigidbody>(true).Where(rb => rb.gameObject.tag == "RagdollPart" && rb != mainRigidbody).ToArray();
        ragdollColliders = GetComponentsInChildren<Collider>(true).Where(col => col.gameObject.tag == "RagdollPart" && col != mainCollider).ToArray();

        DeactivateRagdoll();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            if (isRagdolling == false) ActivateRagdoll();
            else DeactivateRagdoll();
        }
    }

    // Call this method to enable the ragdoll
    public void ActivateRagdoll()
    {
        animator.enabled = false;
        mainRigidbody.isKinematic = true;
        mainCollider.enabled = false;

        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            rb.isKinematic = false;
        }

        foreach (Collider col in ragdollColliders)
        {
            col.enabled = true;
        }

        foreach(MonoBehaviour mb in scriptsToDisable)
        {
            mb.enabled = false;
        }

        isRagdolling = true;
    }

    // Call this method to disable the ragdoll
    public void DeactivateRagdoll()
    {
        Transform centralRagdollPart = hipsLocation;
        if (centralRagdollPart)
        {
            transform.SetPositionAndRotation(centralRagdollPart.position, centralRagdollPart.rotation);
        }
        
        animator.enabled = true;
        mainRigidbody.isKinematic = false;
        mainCollider.enabled = true;

        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            rb.isKinematic = true;
        }

        foreach (Collider col in ragdollColliders)
        {
            col.enabled = false;
        }

        foreach (MonoBehaviour mb in scriptsToDisable)
        {
            mb.enabled = true;
        }

        isRagdolling = false;
    }
}
