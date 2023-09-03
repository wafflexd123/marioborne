using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvestigatePlayerState : MonoBehaviour, IAIState
{
    public List<Transition> transitions { get; set; }
    public AIController controller { get; set; }
    public CoroutineHelper coroutineHelper { get; set; }
    public float maxInvestigateTime = 7f;
    public float minInvestigateTime = 3f;

    public void OnEntry()
    {
        if (controller.soundLocation != null) //only follow sound if patrolling or already investigating
        {
            controller.LastKnownPlayerPosition = controller.soundLocation;
        }
        controller.soundLocation = Vector3.zero;
        coroutineHelper.StartOrAddCoroutine("investigate-timeout", WaitForTime());
    }

    public void OnExit()
    {
        coroutineHelper.CancelCoroutine("investigate-timeout");
    }

    public void Tick()
    {
        controller.MoveTowards(controller.LastKnownPlayerPosition);
        if (controller.soundLocation != null) OnEntry(); //while investigating, sounds will give away position
    }

    public IEnumerator WaitForTime()
    {
        yield return new WaitForSeconds(Random.Range(minInvestigateTime, maxInvestigateTime));
        for (int i = 0; i < transitions.Count; i++)
        {
            if (transitions[i] is ExternalControlTransition)
            {
                ExternalControlTransition t = transitions[i] as ExternalControlTransition;
                t.trigger = true;
                break;
            }
        }
    }
}

