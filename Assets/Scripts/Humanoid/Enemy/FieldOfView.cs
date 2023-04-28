using System.Collections;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
	public float viewRadius;
	[Range(0, 360)]
	public float viewAngle;
	public Transform eyes;
    Transform playerEyes;
	public LayerMask targetMask, obstacleMask;
    public bool canSeePlayer;

	//void Start()
	//{
	//	StartCoroutine(FindTargetsWithDelay(.2f));
	//}

	//IEnumerator FindTargetsWithDelay(float delay)
	//{
	//	while (true)
	//	{
	//		yield return new WaitForSeconds(delay);
	//		FieldOfViewCheck();
	//	}
	//}

	void Update()
	{
		FieldOfViewCheck();
	}

	void FieldOfViewCheck()
	{
		Collider[] rangeChecks = Physics.OverlapSphere(eyes.position, viewRadius, targetMask);
		if (rangeChecks.Length > 0)
		{
			//If transform.position instead of eyes.position is used as the ray origin, it goes through the ground. If the eye height isn't added to the target position, the ray will angle too steeply towards the ground.
			Vector3 dirToTarget = (rangeChecks[0].transform.position + new Vector3(0, eyes.position.y) - eyes.position).normalized;
            playerEyes = rangeChecks[0].transform.GetComponentInParent<Player>().playerEyes;
            if (Vector3.Angle(eyes.forward, dirToTarget) < viewAngle / 2)
            {
                //Debug.DrawLine(eyes.position, playerEyes.position);
                if (!Physics.Linecast(eyes.position, playerEyes.position, obstacleMask))
                {
                    canSeePlayer = true;
                    return;
                }
            }
		}
		if(canSeePlayer) canSeePlayer = false;
	}
}
