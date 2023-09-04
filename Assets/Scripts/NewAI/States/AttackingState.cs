using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackingState : IAIState
{
    public List<Transition> transitions { get; set; }
    public AIController controller { get; set; }
    public CoroutineHelper coroutineHelper { get; set; }
    public enum EnemyType { Standard, Melee, Shield }
    public EnemyType type { get; set; }

    public float TimeBeforePursuingPlayer = 5f;
    protected bool investigateInvoked = false;
    protected Coroutine invCoroutine; //not sure why, but it seems only using default monobehaviour coroutine functions works

    public void OnEntry()
    {
        controller.agent.speed = controller.stateHelper.GetChaseSpeed(type);
        investigateInvoked = false;
        //coroutineHelper.AddCoroutine("investigate", WaitForTime(TimeBeforePursuingPlayer, false));
        controller.transform.LookAt(controller.player.transform.position);
    }

    public void OnExit()
    {
        // stop coroutines
        coroutineHelper.StopAllCoroutines();
        controller.agent.speed = controller.stateHelper.GetDefaultSpeed(type);
        //coroutineHelper.CancelCoroutine("investigate");
    }

    public void Tick()
    {
        controller.MoveTowards(controller.player.transform.position);
        if (controller.fieldOfView.canSeePlayer)
        {
            controller.LastKnownPlayerPosition = controller.player.transform.position;
            controller.AlertOthers();
            if (invCoroutine != null) coroutineHelper.StopCoroutine(invCoroutine);
            investigateInvoked = false;
            if(Vector3.Distance(controller.transform.position, controller.player.transform.position) < controller.stateHelper.GetMeleeDistance(type))
            {
                controller.Fire();
            }
        }
        else
        {
            if (!investigateInvoked)
            {
                invCoroutine = coroutineHelper.StartCoroutine(WaitForTime(TimeBeforePursuingPlayer));
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
    public IEnumerator WaitForTime(float time)
    {
        yield return new WaitForSeconds(time);
        ExternalControlTransition t = transitions[0] as ExternalControlTransition;
        Debug.Log(transitions[0]);
        Debug.Log($"t: {t}, trigger: {t.trigger}");
        t.trigger = true;
    }
}

public class InvestigateTransition : Transition
{
    public bool trigger = false;

    public InvestigateTransition(IAIState targetState) : base(targetState) { }

    public override bool RequirementsMet()
    {
        bool internalTrigger = trigger;
        trigger = false;
        return internalTrigger;
    }
}