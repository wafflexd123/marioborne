using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    public float viewRadius;
    [Range(0, 360)]
    public float viewAngle;
    public Transform eyes;
    [HideInInspector] public Vector3 playerLocation;
    [HideInInspector] Transform playerEyes;
    public LayerMask targetMask, obstacleMask;
    public bool canSeePlayer;

    void Update()
    {
        FieldOfViewCheck();
    }

    void FieldOfViewCheck()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(eyes.position, viewRadius, targetMask, QueryTriggerInteraction.Ignore);
        if (rangeChecks.Length > 0)
        {
            //If transform.position instead of eyes.position is used as the ray origin, it goes through the ground. If the eye height isn't added to the target position, the ray will angle too steeply towards the ground.
            Vector3 dirToTarget = (rangeChecks[0].transform.position + new Vector3(0, eyes.position.y) - eyes.position).normalized;
            playerEyes = rangeChecks[0].transform.GetComponentInParent<Player>().camera.transform;
            if (Vector3.Angle(eyes.forward, dirToTarget) < viewAngle / 2)
            {
                if (!Physics.Linecast(eyes.position, playerEyes.position, obstacleMask, QueryTriggerInteraction.Ignore))
                {
                    canSeePlayer = true;
                    playerLocation = rangeChecks[0].transform.position;
                    return;
                }
            }
        }
        canSeePlayer = false;
    }
}
