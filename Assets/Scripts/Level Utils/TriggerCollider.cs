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
		collider.isTrigger = true;
		if (collider.excludeLayers == 0 || collider.includeLayers == 0) Debug.LogWarning("Trigger collider might not have correct include/exclude layers; might get unintentionally triggered", this);
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

	public static void TriggerDelete(GameObject gameObject)
	{
		Destroy(gameObject);
	}

	public void SetParent(Transform transform)
	{
		this.transform.SetParent(transform);
	}
}
