using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitState : IAIState
{
    public List<Transition> transitions { get; set; }
    public AIController controller { get; set; }
    public float waitBeforePatrollingDuration = 5f;
    public CoroutineHelper coroutineHelper { get; set; }

    public void OnEntry()
    {
        coroutineHelper.StartOrAddCoroutine("waiting", WaitAtLocation());
    }

    public void OnExit()
    {
        coroutineHelper.CancelCoroutine("waiting");
    }

    public void Tick()
    {
        // hmm sound. hmm
    }

    public IEnumerator WaitAtLocation()
    {
        yield return new WaitForSeconds(waitBeforePatrollingDuration);
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
