using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigateFiringPosState : IAIState
{
    public List<Transition> transitions { get; set; }
    public AIController controller { get; set; }
    public StartShootingTransition startShootingTransition { get; set; }
    public CoroutineHelper coroutineHelper { get; set; }

    private List<Vector3> coverPoints;
    private int coverIndex;
    protected Vector3 targetLocation;
    public float TimeBeforeReturningToPatrol = 10f;

    public void OnEntry()
    {
        // Find a target location. 
        // Target location should be somewhere the AI can see the player
        // And somewhere they have some cover
        // Set targetLocation

        // according to Enemy.cs
        //controller.MoveTowards(Player.singlePlayer.transform.position);

        StandardAI standardAI = controller as StandardAI;
        coverPoints = new List<Vector3>();
        for (int i = 0; i < standardAI.coverPoints.childCount - 1; i++)
        {
            coverPoints.Add(standardAI.coverPoints.GetChild(i).position);
        }
        // find closest cover point to go to
        float closestDistance = float.MaxValue;
        for (int i = 0; i < coverPoints.Count; i++)
        {
            if (Vector3.Distance(controller.transform.position, coverPoints[i]) < closestDistance)
            {
                Physics.Raycast(coverPoints[i], standardAI.player.camera.transform.position - coverPoints[i], out RaycastHit hit, Mathf.Infinity, LayerMask.NameToLayer("Enemy"));
                if (hit.collider != null && !hit.collider.gameObject.layer.Equals(LayerMask.NameToLayer("Player")))
                { //raycast to find out whether cover is valid (cover can see player)
                    coverIndex = i;
                    closestDistance = Vector3.Distance(controller.transform.position, coverPoints[i]);
                }
            }
        }
        targetLocation = coverPoints[coverIndex];
        coroutineHelper.AddCoroutine("returnToPatrol-Navi", WaitForTime(TimeBeforeReturningToPatrol));
    }

    public void OnExit() { }

    public void Tick()
    {
        // Move towards target location
        // *** REPLACE THIS, should instead go towards its decided target location. 
        controller.MoveTowards(targetLocation);
        startShootingTransition.destination = targetLocation;
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
    public void SetCoverPoints(List<Vector3> coverPoints)
    {
        this.coverPoints = new List<Vector3>() { };
        for (int i = 0; i < coverPoints.Count; i++)
            this.coverPoints.Add(coverPoints[i]);
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