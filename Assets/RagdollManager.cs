using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RagdollManager : MonoBehaviour
{
    public Rigidbody mainRigidbody;
    public Collider mainCollider;
    public Rigidbody[] ragdollRigidbodies;
    private Collider[] ragdollColliders;
    private Animator enemyAnimator;
    private NavMeshAgent navMeshAgent;
    public bool isCheckingGround = false;
    [SerializeField] private MonoBehaviour[] componentsToDisable;
    private Transform ragdollHips;

    private void Start()
    {
        ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
        ragdollColliders = GetComponentsInChildren<Collider>();
        enemyAnimator = GetComponent<Animator>();
        navMeshAgent = GetComponentInParent<NavMeshAgent>();
        ragdollHips = enemyAnimator.GetBoneTransform(HumanBodyBones.Hips);
        ToggleRagdoll(false);
    }

    public void ToggleRagdoll(bool state)
    {
        enemyAnimator.enabled = !state;
        navMeshAgent.enabled = !state;
        mainRigidbody.isKinematic = state;
        mainCollider.enabled = !state;

        foreach (MonoBehaviour mb in componentsToDisable)
        {
            mb.enabled = !state;
        }
        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            rb.isKinematic = !state;
            rb.useGravity = !state;
        }
        foreach (Collider collider in ragdollColliders)
        {
            collider.enabled = state;
        }
    }
    public IEnumerator GroundCheck()
    {
        yield return new WaitForSeconds(0.5f);

        isCheckingGround = true;
        while (true)
        {
            if (mainRigidbody.velocity.magnitude < 0.1f)
            {
                ToggleRagdoll(false);
                isCheckingGround = false;
                yield break;
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void AlignPositionToHips()
    {
        Vector3 originalHipsPosition = ragdollHips.position;
        transform.position = ragdollHips.position;

        if(Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit))
        {
            transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
        }

        ragdollHips.position = originalHipsPosition;
    }
}

