using UnityEngine;

public class Sniper : Gun
{
	protected LineRenderer line;

	protected void Awake()
	{
		base.Start();
		line = GetComponent<LineRenderer>();
		line.widthMultiplier = 0.05f;
		line.enabled = false;
		enabled = false;
	}

	protected override void OnPickup()
	{
		base.OnPickup();
		if (wielder is AIController)
		{
			wielder.model.sniperLayer = true;
			wielder.model.holdingSniper = true;
			line.enabled = true;
			enabled = true;
		}
	}

	void FixedUpdate()
	{
		Physics.Raycast(firePosition.position, wielder.LookingAt - firePosition.position, out RaycastHit hit);
		line.SetPositions(new Vector3[] { firePosition.position, hit.point });
	}

	protected override void OnWielderChange()
	{
		base.OnWielderChange();
		if (wielder is AIController)
		{
			wielder.model.holdingSniper = false;
			wielder.model.sniperLayer = false;
			line.enabled = false;
			enabled = false;
		}
	}
}
