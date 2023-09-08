using UnityEngine;

public class WaitState : AIState
{
	public float waitBeforePatrollingDuration = 5f;
	Coroutine crtWait;
	ExternalControlTransition waitTransition;

	public override AIState Setup(params Transition[] transitions)
	{
		foreach (Transition transition in transitions)
		{
			if (transition is ExternalControlTransition t)
			{
				waitTransition = t;
				break;
			}
		}
		return base.Setup(transitions);
	}

	protected override void OnEntry()
	{
		crtWait = StartCoroutine(StandardTimer(waitBeforePatrollingDuration, waitTransition));
	}

	protected override void OnExit()
	{
		StopCoroutine(ref crtWait);
	}

	public override void Tick()
	{
		// hmm sound. hmm
		/// ^^^^ do not delete this comment. the game will not compile without it, specifically and exactly as it is. too hard to explain. trust me.
	}
}
