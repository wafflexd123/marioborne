using System.Collections;
using UnityEngine;

public class SniperSweepState : AIState
{
	public float rotationSpeed;
	public float sweepLimit = 65f;
	public float sweepDirection = 1f;

	private float currentAngle;
	private float startAngle;
	private bool angleSet = false;

	protected override void OnEntry()
	{
		if(!angleSet) //sets the starting angle to refer back to
        {
			controller = controller == null ? gameObject.GetComponent<AIController>() : controller;
			startAngle = controller.transform.eulerAngles.y;
			currentAngle = startAngle;
			angleSet = true;
		}
        else currentAngle = controller.transform.eulerAngles.y;
	}

    public override void Tick()
	{
		if (controller.transform.eulerAngles.y > startAngle - 1f && controller.transform.eulerAngles.y < startAngle + 1f) angleSet = true; //checks if enemy has rotated back to starting angle

		if (angleSet) Sweep();
		else
		{
			if(controller.transform.eulerAngles.y < startAngle - 1f) ReOrient(1); //reorients enemy back to starting rotation,
			if(controller.transform.eulerAngles.y > startAngle + 1f) ReOrient(-1); //depending on direction
		}
	}

	protected void Sweep() //does a sweeping motion across a specified angle (sweepLimit)
	{
		currentAngle += rotationSpeed * sweepDirection * Time.deltaTime;
		if (currentAngle < startAngle - sweepLimit || currentAngle > startAngle + sweepLimit) sweepDirection *= -1f;
		controller.transform.rotation = Quaternion.Euler(0f, currentAngle, 0f);
	}

	protected void ReOrient(float direction) //rotates backward depending on direction
    {
		currentAngle += controller.RotationSpeed * direction * Time.deltaTime;
		controller.transform.rotation = Quaternion.Euler(0f, currentAngle, 0f);
    }
}