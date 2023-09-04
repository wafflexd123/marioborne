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
    private int sightCoverIndex;
    protected Vector3 targetLocation;
    protected StandardAI standardAI;
    public ExternalControlTransition returnToPatrolTransition;
    // settings
    public float TimeBeforeReturningToPatrol = 10f;
    protected float lookingDistance = 3f;   // will start to rotate to look at last known player position when this close to firing position

    public void OnEntry()
    {
        // Find a target location. 
        // Target location should be somewhere the AI can see the player
        // And somewhere they have some cover
        // Set targetLocation
        //coroutineHelper.AddCoroutine("returnToPatrol-Navi", WaitForTime(TimeBeforeReturningToPatrol));

        standardAI = controller as StandardAI;

        if (standardAI.GetCoverPriority() == CoverPriority.IgnoreCover)
        {
            targetLocation = standardAI.transform.position;
            return;
        }

        controller.agent.speed = standardAI.runToCoverSpeed;
        coverPoints = new List<Vector3>();
        for (int i = 0; i < standardAI.coverPoints.childCount - 1; i++)
        {
            if (Vector3.Distance(standardAI.coverPoints.GetChild(i).position, controller.transform.position) > standardAI.relocateDistance)
                coverPoints.Add(standardAI.coverPoints.GetChild(i).position);
        }
        // find closest cover point to go to
        float closestDistance = float.MaxValue;
        float closestSightDistance = float.MaxValue;
        for (int i = 0; i < coverPoints.Count; i++)
        {
            if (Vector3.Distance(controller.transform.position, coverPoints[i]) < closestSightDistance)
            {
                Physics.Raycast(new Vector3(coverPoints[i].x, coverPoints[i].y + 1.6f, coverPoints[i].z), standardAI.player.transform.position - coverPoints[i], out RaycastHit player, Mathf.Infinity, LayerMask.NameToLayer("Enemy"));
                if (player.collider != null && player.collider.gameObject.layer.Equals(LayerMask.NameToLayer("Player")))
                { //tries to find cover where the enemy can also see the player (enemy height taken into account)
                    Physics.Raycast(coverPoints[i], standardAI.player.camera.transform.position - coverPoints[i], out RaycastHit cover, Mathf.Infinity, LayerMask.NameToLayer("Enemy"));
                    if (cover.collider != null && !cover.collider.gameObject.layer.Equals(LayerMask.NameToLayer("Player")))
                    {
                        sightCoverIndex = i;
                        closestSightDistance = Vector3.Distance(controller.transform.position, coverPoints[i]);
                    }
                }
            }
            if (Vector3.Distance(controller.transform.position, coverPoints[i]) < closestDistance)
            {
                Physics.Raycast(coverPoints[i], standardAI.player.camera.transform.position - coverPoints[i], out RaycastHit cover, Mathf.Infinity, LayerMask.NameToLayer("Enemy"));
                if (cover.collider != null && !cover.collider.gameObject.layer.Equals(LayerMask.NameToLayer("Player")))
                { //raycast to find out whether cover is valid (cover cannot see player)

                    coverIndex = i;
                    closestDistance = Vector3.Distance(controller.transform.position, coverPoints[i]);
                }
            }

        }

        if (standardAI.GetCoverPriority() == CoverPriority.RequireCover)
        {
            if (closestSightDistance < float.MaxValue) targetLocation = coverPoints[sightCoverIndex];
            else if (closestDistance < float.MaxValue) targetLocation = coverPoints[coverIndex];
            else targetLocation = controller.transform.position; //if there is no cover, should they just stand in place?
        }
        else // CoverPrioirty.IfNear
        {
            if (closestSightDistance <= standardAI.GetNearCoverDistance()) targetLocation = coverPoints[sightCoverIndex];
            else if (closestDistance <= standardAI.GetNearCoverDistance()) targetLocation = coverPoints[coverIndex];
            else targetLocation = controller.transform.position; //if there is no cover, should they just stand in place?
        }
    }

    public void OnExit() 
    {
        controller.agent.speed = standardAI.defaultSpeed;
        startShootingTransition.destination = Vector3.zero;
        startShootingTransition.position = Vector3.one; //resets requirements, otherwise the guy goes a bit wonky
        //coroutineHelper.CancelCoroutine("returnToPatrol-Navi");
        coroutineHelper.CancelTimer("returnToPatrol");
    }

    public void Tick()
    {
        controller.MoveTowards(targetLocation);
        startShootingTransition.destination = targetLocation;
        startShootingTransition.position = controller.transform.position;

        if (Vector3.Distance(controller.transform.position, targetLocation) < lookingDistance)
        {
            controller.RotateTowards(controller.LastKnownPlayerPosition);
        }

        //needs work i think
        if (!controller.fieldOfView.canSeePlayer)
        {
            //coroutineHelper.StartOrAddCoroutine("returnToPatrol-Navi", WaitForTime(TimeBeforeReturningToPatrol));
            //coroutineHelper.StartKnownCoroutine("returnToPatrol-Navi");
            coroutineHelper.StartTimer("returnToPatrol", TimeBeforeReturningToPatrol, returnToPatrolTransition);
        }
        else
        {
            //coroutineHelper.CancelCoroutine("returnToPatrol-Navi");
            coroutineHelper.CancelTimer("returnToPatrol");
        }
    }

    public IEnumerator WaitForTime(float time)
    {
        //yield return new WaitForSeconds(time);
        yield return new WaitForSeconds(TimeBeforeReturningToPatrol);
        Debug.Log($"Navigate firing pos WaitForTime has waited its time: {TimeBeforeReturningToPatrol}");
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

    //idk if we'll need this at some point
    /* public void SetCoverPoints(List<Vector3> coverPoints)
    {
        this.coverPoints = new List<Vector3>() { };
        for (int i = 0; i < coverPoints.Count; i++)
            this.coverPoints.Add(coverPoints[i]);
    } */
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