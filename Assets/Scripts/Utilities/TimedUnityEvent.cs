using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TimedUnityEvent : UnityEventHelper
{
	public bool beginOnEnable;
	public List<TimedEvent> events = new List<TimedEvent>();
	Coroutine coroutine;

	private void OnEnable()
	{
		if (beginOnEnable) BeginTimedEvents();
	}

	public void BeginTimedEvents()
	{
		if (coroutine == null) coroutine = StartCoroutine(E());
		else Debug.LogWarning("Timed event was fired more than once!", this);
		IEnumerator E()
		{
			for (int i = 0; i < events.Count; i++)
			{
				yield return new WaitForSeconds(events[i].delay);
				events[i].unityEvent.Invoke();
			}
			coroutine = null;
		}
	}

	[System.Serializable]
	public class TimedEvent
	{
		public float delay;
		public UnityEvent unityEvent;
	}
}
