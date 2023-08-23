using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveShootingState : IAIState
{
    public List<Transition> transitions { get; set; }
    public AIController controller { get; set; }
    public CoroutineHelper coroutineHelper { get; set; }

    public float TimeBeforeRelocating = 10f;
    public float TimeBeforePursuingPlayer = 4f;

    protected bool investigateInvoked = false;
    protected bool relocateInvoked = false;
    protected StandardAI standardAI;

    public void OnEntry()
    {
        standardAI = controller as StandardAI;
        investigateInvoked = false;
        relocateInvoked = false;
        controller.transform.LookAt(controller.player.transform.position);
    }   

    public void OnExit()
    {
        // stop coroutines
        coroutineHelper.CancelCoroutine("relocate");
        coroutineHelper.CancelCoroutine("investigate");
    }

    public void Tick()
    {
        if (controller.fieldOfView.canSeePlayer)
        {
            controller.transform.LookAt(controller.player.transform.position);
            controller.Fire();
            controller.LastKnownPlayerPosition = controller.player.transform.position;
            if (!relocateInvoked)
            {
                coroutineHelper.StartOrAddCoroutine("relocate", WaitForTime(TimeBeforeRelocating, true));
                relocateInvoked = true;
                //Invoke("Relocate", TimeBeforeRelocating); // man I wish I could just do this, but its only on monobehaviours
            }
            coroutineHelper.CancelCoroutine("investigate");
            investigateInvoked = false;
        }
        else
        {
            coroutineHelper.CancelCoroutine("relocate");
            relocateInvoked = false;
            if (!investigateInvoked)
            {
                Debug.Log("investigate proc");
                coroutineHelper.StartOrAddCoroutine("investigate", WaitForTime(TimeBeforePursuingPlayer, false));
                investigateInvoked = true;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="time"></param>
    /// <param name="relocation">If true this will trigger relocation on complete, otherwise it will trigger investigating the player. </param>
    /// <returns></returns>
    public IEnumerator WaitForTime(float time, bool relocation)
    {
        yield return new WaitForSeconds(time);
        /*if (relocation) //relocation not implemented atm
        {
            ExternalControlTransition t = transitions[1] as ExternalControlTransition;
            t.trigger = true;
        }
        else*/
        if(!relocation)
        {
            ExternalControlTransition t = transitions[0] as ExternalControlTransition;
            t.trigger = true;
        }
    }
}

public class ExternalControlTransition : Transition
{
    public bool trigger = false;

    public ExternalControlTransition(IAIState targetState) : base(targetState) { }

    public override bool RequirementsMet()
    {
        bool internalTrigger = trigger;
        trigger = false;
        return internalTrigger;
    }
}