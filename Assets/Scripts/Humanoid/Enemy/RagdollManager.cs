using System.Linq;
using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class RagdollManager : MonoBehaviour
{
    [Header("Ragdoll Fields")]
    [SerializeField] private Transform hipsLocation;
    private Vector3 originalHipsLocalPosition;
    private Animator animator;
    private Rigidbody mainRigidbody;
    private Collider mainCollider;
    private NavMeshAgent navMeshAgent;
    private Rigidbody[] ragdollRigidbodies;
    private Collider[] ragdollColliders;
    [SerializeField] private MonoBehaviour[] scriptsToDisable;

    private bool isRagdollActive;
    private bool followHips = false;
    private float timeWithZeroVelocity = 0f;
    [SerializeField] private float delayBeforeDeactivation = 5f;

    // Struggle Code
    [Header("Struggle Fields")]
    private bool isStruggling = false;
    [SerializeField] private float struggleIntensity = 5f;
    [SerializeField] private float struggleFrequency = 0.5f;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        mainRigidbody = GetComponent<Rigidbody>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        mainCollider = GetComponentsInChildren<Collider>(true).FirstOrDefault(col => col.gameObject.tag == "MainCollider");
        originalHipsLocalPosition = hipsLocation.localPosition;
        ragdollRigidbodies = GetComponentsInChildren<Rigidbody>(true).Where(rb => rb.gameObject.tag == "RagdollPart" && rb != mainRigidbody).ToArray();
        ragdollColliders = GetComponentsInChildren<Collider>(true).Where(col => col.gameObject.tag == "RagdollPart" && col != mainCollider).ToArray();

        DeactivateRagdoll();
    }

    private void Update()
    {
        if (followHips)
        {
            Vector3 desiredPosition = transform.TransformPoint(originalHipsLocalPosition);
            hipsLocation.position = Vector3.Lerp(hipsLocation.position, desiredPosition, 5 * Time.deltaTime); // Adjust 'lerpSpeed' to fit your needs.
        }
    }


    // Call this method to enable the ragdoll
    public void ActivateRagdoll()
    {
        animator.enabled = false;
        //mainRigidbody.isKinematic = true;
        mainCollider.enabled = false;
        navMeshAgent.enabled = false;

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

        isRagdollActive = true;

        StartCoroutine(CheckVelocityAndGrounded());
        SetStruggle(true);
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
        //mainRigidbody.isKinematic = false;
        mainCollider.enabled = true;
        navMeshAgent.enabled = true;

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

        isRagdollActive = false;

        SetStruggle(false);
    }

    private IEnumerator CheckVelocityAndGrounded()
    {
        while (isRagdollActive)
        {
            if (IsGrounded() && mainRigidbody.velocity == Vector3.zero)
            {
                timeWithZeroVelocity += Time.deltaTime;
                if (timeWithZeroVelocity >= delayBeforeDeactivation)
                {
                    DeactivateRagdoll();
                    timeWithZeroVelocity = 0f;
                    yield break;
                }
            }
            else
            {
                timeWithZeroVelocity = 0f;
            }
            yield return null;
        }
    }

    // Check if the enemy is grounded using a Raycast from the main collider
    private bool IsGrounded()
    {
        if (hipsLocation == null) return false;

        float distanceToGround = hipsLocation.localScale.y / 2;
        Vector3 start = hipsLocation.position;
        Vector3 direction = -Vector3.up;

        Debug.DrawRay(start, direction * (distanceToGround + 0.1f), Color.red, 1f);

        return Physics.Raycast(start, direction, distanceToGround + 0.1f);
    }

    public void SetRagdollGravity(bool state)
    {
        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            rb.useGravity = state;
        }
    }

    public void SetStruggle(bool state)
    {
        isStruggling = state;
        if (isStruggling)
        {
            StartCoroutine(StruggleMovement());
        }
    }

    private IEnumerator StruggleMovement()
    {
        while (isStruggling && isRagdollActive)
        {
            foreach (Rigidbody rb in ragdollRigidbodies)
            {
                Vector3 randomForce = new Vector3(
                    UnityEngine.Random.Range(-struggleIntensity, struggleIntensity),
                    UnityEngine.Random.Range(-struggleIntensity, struggleIntensity),
                    UnityEngine.Random.Range(-struggleIntensity, struggleIntensity)
                );
                rb.AddForce(randomForce, ForceMode.Impulse);
            }
            yield return new WaitForSeconds(struggleFrequency);
        }
    }

    public void ToggleHipFollow()
    {
        followHips = !followHips;
    }
}
