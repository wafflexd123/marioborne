using UnityEngine;
using UnityEngine.Events;

public class TriggerCollider : MonoBehaviour
{
	public UnityEvent onEnter, onStay, onExit;
	[HideInInspector] public bool isTriggered;
	[HideInInspector] public Collider other;

	private void Awake()
	{
		Collider collider = GetComponent<Collider>();
		if (gameObject.layer == 0) gameObject.layer = 13;//ignore bullets layer
		if (collider.excludeLayers == 0) collider.excludeLayers = 1 << 13;//ignore bullets layer
		if (collider.includeLayers == 0) collider.includeLayers = ~(1 << 13);//ignore bullets layer
	}

	private void OnTriggerEnter(Collider other)
	{
		isTriggered = true;
		this.other = other;
		onEnter.Invoke();
	}

	private void OnTriggerStay(Collider other)
	{
		onStay.Invoke();
	}

	private void OnTriggerExit(Collider other)
	{
		onExit.Invoke();
		isTriggered = false;
		if (this.other == other) this.other = null;
	}
}
