using UnityEngine;

public abstract class Humanoid : MonoBehaviourPlus
{
	public Transform hand;
	public RaycastHit raycast;

	/// <returns>True if button was pressed this frame</returns>
	public abstract bool GetAxisDown(string axis, out float value);

	/// <returns>Value of axis if button is held</returns>
	public float GetAxis(string axis)
	{
		GetAxisDown(axis, out float value);
		return value;
	}
}
