using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolState : IAIState
{
    public List<Transition> transitions { get; set; }
    public AIController controller { get; set; }

    private List<Vector3> patrolPoints;
    private int patrolIndex = 0;
    public bool pingpong = true;
    protected int pingpongDirection = 1;

    public void OnEntry()
    {
        StandardAI standardAI = controller as StandardAI;
        patrolPoints = new List<Vector3>();
        for(int i=0; i< standardAI.patrolPoints.childCount-1; i++)
        {
            patrolPoints.Add(standardAI.patrolPoints.GetChild(i).position);
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
        //Debug.Log($"Controller is valid: {controller != null}");
        //Debug.Log(controller.transform.position);
        //Debug.Log($"patrolPoints is valid: {patrolPoints != null}");
        //Debug.Log(patrolPoints[patrolIndex]);
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
    public void SetPatrolPoints(List<Vector3> patrolPoints) 
    { 
        this.patrolPoints = new List<Vector3>(){}; 
        for (int i = 0; i < patrolPoints.Count; i++)
            this.patrolPoints.Add(patrolPoints[i]);
    }
    public void SetPatrolPoints(Transform[] patrolPoints)
    {
        this.patrolPoints = new List<Vector3>() { };
        for (int i = 0; i < patrolPoints.Length; i++)
            this.patrolPoints.Add(patrolPoints[i].position);
    }
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

public class CanHearPlayerTransition : Transition
{
    public AIController controller;

    public CanHearPlayerTransition(IAIState targetState, AIController controller) : base(targetState)
    {
        this.controller = controller;
    }

    public override bool RequirementsMet()
    {
        return controller.soundLocation != null;
    }
}