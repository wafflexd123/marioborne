using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAIState
{
    protected virtual void Awake()
	{
        controller = gameObject.GetComponent<AIController>();
	}

    protected IEnumerator StandardTimer(float duration, ExternalControlTransition externalControlTransition)
    {
        yield return new WaitForSeconds(duration);
        externalControlTransition.trigger = true;
    }

    void Tick();
    void OnEntry();
    void OnExit();
    public List<Transition> transitions { get; set; }
    public AIController controller { get; set; }
    public GameObject gameObject { get; }
}
