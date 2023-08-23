using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolState : IAIState
{
    public List<Transition> transitions { get; set; }
    public AIController controller { get; set; }

    private List<Vector3> patrolPoints;
    private int patrolIndex;
    public bool pingpong = true;
    protected int pingpongDirection = 1;

    public void OnEntry()
    {
        for(int i=0; i<controller.patrolPoints.childCount-1; i++)
        {
            patrolPoints[i] = controller.patrolPoints.GetChild(i).transform.position;
        }
        // find closest patrol points to go to next. 
        float closestDistance = float.MaxValue;
        for (int i=0; i<patrolPoints.Count; i++)
        {
            if (Vector3.Distance(controller.transform.position, patrolPoints[i]) < closestDistance)
            {
                patrolIndex = i;
                closestDistance = Vector3.Distance(controller.transform.position, patrolPoints[i]);
            }
        }
    }

    public void OnExit()
    {
        
    }

    public void Tick()
    {
        if (Vector3.Distance(controller.transform.position, patrolPoints[patrolIndex]) < 0.05f)
            IncrementPatrolIndex();
        controller.MoveTowards(patrolPoints[patrolIndex]);
    }

    //public void Setup()
    //{
    //    transitions = new List<Transition>();
    //    CanSeePlayerTransition NavigateFiring = new CanSeePlayerTransition(controller);
    //    transitions.Add(NavigateFiring);
    //}

    protected void IncrementPatrolIndex()
    {
        if (pingpong)
        {
            //if (patrolIndex - 1 == -1 || patrolIndex + 1 == patrolPoints.Count)
            if (patrolIndex + pingpongDirection == -1 || patrolIndex + pingpongDirection == patrolPoints.Count)
                pingpongDirection *= -1;
            patrolIndex = patrolIndex + pingpongDirection;
        }
        else
            patrolIndex = (patrolIndex + 1) % patrolPoints.Count;
    }
    public void SetPatrolPoints(List<Vector3> patrolPoints) { this.patrolPoints = patrolPoints; }
}

public class CanSeePlayerTransition : Transition
{
    public AIController controller;

    public CanSeePlayerTransition(IAIState targetState, AIController controller) : base(targetState)
    {
        this.controller = controller;
    }

    public override bool RequirementsMet()
    {
        return controller.fieldOfView.canSeePlayer;
    }
}