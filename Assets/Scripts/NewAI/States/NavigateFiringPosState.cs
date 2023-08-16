using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigateFiringPosState : IAIState
{
    public List<Transition> transitions { get; set; }
    public AIController controller { get; set; }
    public StartShootingTransition startShootingTransition { get; set; }
    public CoroutineHelper coroutineHelper { get; set; }

    protected Vector3 targetLocation;
    public float TimeBeforeReturningToPatrol = 10f;

    public void OnEntry()
    {
        // Find a target location. 
        // Target location should be somewhere the AI can see the player
        // And somewhere they have some cover
        // Set targetLocation

        // according to Enemy.cs
        controller.MoveTowards(Player.singlePlayer.transform.position);
    }

    public void OnExit() { }

    public void Tick()
    {
        // Move towards target location
        // *** REPLACE THIS, should instead go towards its decided target location. 
        controller.MoveTowards(Player.singlePlayer.transform.position);
        startShootingTransition.destination = Player.singlePlayer.transform.position;
        startShootingTransition.position = controller.transform.position;

        // keep this part
        if (!controller.fieldOfView.canSeePlayer)
        {
            coroutineHelper.StartOrAddCoroutine("returnToPatrol-Navi", WaitForTime(TimeBeforeReturningToPatrol));
        }
        else
        {
            coroutineHelper.CancelCoroutine("returnToPatrol-Navi");
        }
    }

    public IEnumerator WaitForTime(float time)
    {
        yield return new WaitForSeconds(time);
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

public class StartShootingTransition : Transition
{
    public Vector3 destination = Vector3.zero;
    public Vector3 position = Vector3.one;

    public StartShootingTransition(IAIState targetState) : base(targetState) { }

    public override bool RequirementsMet()
    {
        return Vector3.Distance(destination, position) < 0.05f;
    }
}