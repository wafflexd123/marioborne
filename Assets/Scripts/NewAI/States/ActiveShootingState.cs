using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveShootingState : MonoBehaviour, IAIState
{
    public List<Transition> transitions { get; set; }
    public AIController controller { get; set; }

    public float TimeBeforeRelocating = 10f;
    public float TimeBeforePursuingPlayer = 5f;

    protected bool investigateInvoked = false;
    protected bool relocateInvoked = false;
    protected Coroutine invCoroutine; //not sure why, but it seems only using default monobehaviour coroutine functions works
    protected Coroutine relCoroutine;
    protected StandardAI standardAI;

    public void OnEntry()
    {
        investigateInvoked = false;
        relocateInvoked = false;
        //coroutineHelper.AddCoroutine("investigate", WaitForTime(TimeBeforePursuingPlayer, false));
        //coroutineHelper.AddCoroutine("relocate", WaitForTime(TimeBeforeRelocating, true));
        controller.transform.LookAt(controller.player.transform.position);
    }   

    public void OnExit()
    {
        // stop coroutines
        coroutineHelper.StopAllCoroutines();
        //coroutineHelper.CancelCoroutine("relocate");
        //coroutineHelper.CancelCoroutine("investigate");
    }

    public void Tick()
    {
        if (controller.fieldOfView.canSeePlayer)
        {
            
            controller.transform.LookAt(controller.player.transform.position);
            controller.Fire();
            controller.LastKnownPlayerPosition = controller.player.transform.position;
            controller.AlertOthers();
            if (!relocateInvoked)
            {
                relCoroutine = coroutineHelper.StartCoroutine(WaitForTime(TimeBeforeRelocating, true));
                relocateInvoked = true;
                //Invoke("Relocate", TimeBeforeRelocating); // man I wish I could just do this, but its only on monobehaviours
            }
            if(invCoroutine != null) coroutineHelper.StopCoroutine(invCoroutine);
            investigateInvoked = false;
        }
        else
        {
            //if (relCoroutine != null) coroutineHelper.StopCoroutine(relCoroutine);
            relocateInvoked = false;
            if (!investigateInvoked)
            {
                invCoroutine = coroutineHelper.StartCoroutine(WaitForTime(TimeBeforePursuingPlayer, false));
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
        if (relocation) //currently relocation just swaps the two closest positions. any idea on how we can improve it?
        {
            ExternalControlTransition t = transitions[1] as ExternalControlTransition;
            t.trigger = true;
        }
        else
        {
            ExternalControlTransition t = transitions[0] as ExternalControlTransition;
            t.trigger = true;
        }
    }
}

