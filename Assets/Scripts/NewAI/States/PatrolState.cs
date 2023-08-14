using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolState : IAIState
{
    public List<Transition> transitions { get; set; }
    //public AIController controller { get; set; }

    private List<Vector3> patrolPoints;
    private int patrolIndex;
    public bool pingPong = true;

    public void OnEntry()
    {
        throw new System.NotImplementedException();
    }

    public void OnExit()
    {
        throw new System.NotImplementedException();
    }

    public void Tick()
    {
        throw new System.NotImplementedException();
    }

    //public void Setup()
    //{
    //    transitions = new List<Transition>();
    //    CanSeePlayerTransition NavigateFiring = new CanSeePlayerTransition(controller);
    //    transitions.Add(NavigateFiring);
    //}

    public void SetPatrolPoints(List<Vector3> patrolPoints) { this.patrolPoints = patrolPoints; }
}

public class CanSeePlayerTransition : Transition
{
    public AIController controller;

    public CanSeePlayerTransition(AIController controller)
    {
        this.controller = controller;
    }

    public override bool RequirementsMet()
    {
        return controller.fieldOfView.canSeePlayer;
    }
}