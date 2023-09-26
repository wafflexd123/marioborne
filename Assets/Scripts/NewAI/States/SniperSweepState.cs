using System.Collections;
using UnityEngine;

public class SniperSweepState : AIState
{
	//Inspector
	public float rotationSpeed;
	//Script
	public float sweepLimit = 65f;
	private float currentAngle;
	private float startAngle;
	private bool angleSet = false;
	private float sweepDirection = 1f;

	protected override void OnEntry()
	{
		if(!angleSet)
        {
			startAngle = controller.transform.eulerAngles.y;
			angleSet = true;
		}
		currentAngle = startAngle;
	}

    public override void Tick()
	{
		Sweep();
	}

	protected void Sweep()
	{
		currentAngle += controller.rotationSpeed * sweepDirection * Time.deltaTime;
		if (currentAngle < startAngle- sweepLimit || currentAngle > startAngle + sweepLimit)
        {
			sweepDirection *= -1f;
        }
		controller.transform.rotation = Quaternion.Euler(0f, currentAngle, 0f);
	}
}