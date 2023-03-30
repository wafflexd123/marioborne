using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    public float viewRadius;
    [Range(0, 360)]
    public float viewAngle;

    public GameObject playerRef;

    public LayerMask targetMask, obstacleMask;
    public List<Transform> visibleTargets = new List<Transform>();

    public bool canSeePlayer;

    // Start is called before the first frame update
    void Start()
    {
        playerRef = GameObject.FindGameObjectWithTag("Player");
        StartCoroutine("FindTargetsWithDelay", 0.2f);
    }

    // Update is called once per frame
    void Update()
    {
        if (canSeePlayer) Debug.Log("can see player");
        else Debug.Log("lost player");
    }

    IEnumerator FindTargetsWithDelay(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            FieldOfViewCheck();
        }
    }

    void FieldOfViewCheck()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, viewRadius, targetMask);

        if(rangeChecks.Length > 0)
        {
            Transform target = rangeChecks[0].transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2)
            {
                float distToTarget = Vector3.Distance(transform.position, target.position);


                if (!Physics.Raycast(transform.position, dirToTarget, distToTarget, obstacleMask))
                {
                    canSeePlayer = true;
                }
                else canSeePlayer = false;
            }
            else canSeePlayer = false;
        }
        else if (canSeePlayer)
        {
            canSeePlayer = false;
        }
    }
}
