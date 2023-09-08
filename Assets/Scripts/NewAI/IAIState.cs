using System.Collections;

public abstract class AIState : MonoBehaviourPlus
{
	protected Transition[] transitions;
	protected AIController controller;

	protected virtual void Awake()
	{
		controller = gameObject.GetComponent<AIController>();
	}

	public virtual AIState Setup(params Transition[] transitions)
	{
		this.transitions = transitions;
		return this;
	}

	public bool TryTransition(out AIState newState)
	{
		foreach (Transition transition in transitions)
		{
			if (transition.RequirementsMet())
			{
				OnExit();
				transition.targetState.OnEntry();
				newState = transition.targetState;
				return true;
			}
		}
		newState = null;
		return false;
	}

	public AIState BeginState()
	{
		OnEntry();
		return this;
	}

	protected IEnumerator StandardTimer(float duration, ExternalControlTransition externalControlTransition)
	{
		yield return new WaitForSeconds(duration);
		externalControlTransition.trigger = true;
	}

	public virtual void Tick() { }
	protected virtual void OnEntry() { }
	protected virtual void OnExit() { }
}
