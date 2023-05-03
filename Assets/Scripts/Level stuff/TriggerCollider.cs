using UnityEngine;
using UnityEngine.Events;

public class TriggerCollider : MonoBehaviour
{
	public UnityEvent onEnter, onStay, onExit;
	[HideInInspector] public bool isTriggered;

	private void OnTriggerEnter(Collider other)
	{
		onEnter.Invoke();
		isTriggered = true;
	}

	private void OnTriggerStay(Collider other)
	{
		onStay.Invoke();
	}

	private void OnTriggerExit(Collider other)
	{
		onExit.Invoke();
		isTriggered = false;
	}
}
