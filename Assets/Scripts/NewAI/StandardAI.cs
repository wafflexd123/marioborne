using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.VersionControl.Asset;

public class StandardAI : AIController
{
    [SerializeField, Tooltip("Whether the patrol path should be run in reverse when completed, or looping from the 0th patrol point")] 
    protected bool patrolPingPong = true;
    [SerializeField] public Transform patrolPoints;
    [SerializeField] public Transform coverPoints;
    public float defaultSpeed;
    public float runToCoverSpeed; //will extend these for each state if needed
    public float relocateDistance = 0.5f;

    // AI States
    protected PatrolState patrolState;
    protected NavigateFiringPosState navigateFiringPosState;
    protected InvestigatePlayerState investigatePlayerState;
    protected ActiveShootingState activeShootingState;
    protected WaitState waitState;

    // helpers
    protected CoroutineHelper coroutineHelper;
    protected PatrolSetup patrolSetup;
    
    protected override void Awake()
    {
        base.Awake();
        try { coroutineHelper = gameObject.GetComponent<CoroutineHelper>(); }
        catch { coroutineHelper = gameObject.AddComponent<CoroutineHelper>(); }
        try { patrolSetup = gameObject.GetComponent<PatrolSetup>(); }
        catch { patrolSetup = gameObject.AddComponent<PatrolSetup>(); }

        states = new List<IAIState>();
        patrolState ??= new PatrolState();
        patrolState.controller = this;
        states.Add(patrolState);
        navigateFiringPosState = new NavigateFiringPosState();
        navigateFiringPosState.controller = this;
        states.Add(navigateFiringPosState);
        investigatePlayerState = new InvestigatePlayerState();
        investigatePlayerState.controller = this;
        states.Add(investigatePlayerState);
        activeShootingState = new ActiveShootingState();
        activeShootingState.controller = this;
        states.Add(activeShootingState);
        waitState = new WaitState();
        waitState.controller = this;
        states.Add(waitState);

        CanSeePlayerTransition navigateFiring = new CanSeePlayerTransition(navigateFiringPosState, this);
        CanHearPlayerTransition investigateSound = new CanHearPlayerTransition(investigatePlayerState, this);
        patrolState.transitions = new List<Transition>() { navigateFiring, investigateSound };
        // reach destination -> active shooting
        StartShootingTransition startShootingTransition = new StartShootingTransition(activeShootingState);
        navigateFiringPosState.startShootingTransition = startShootingTransition;
        // 10s pass -> patrol
        ExternalControlTransition returnToPatrolNavi = new ExternalControlTransition(patrolState);
        navigateFiringPosState.coroutineHelper = coroutineHelper;
        navigateFiringPosState.transitions = new List<Transition>() { returnToPatrolNavi, startShootingTransition };

        ExternalControlTransition investigateTransition = new ExternalControlTransition(investigatePlayerState);
        ExternalControlTransition relocateTransition = new ExternalControlTransition(navigateFiringPosState);
        activeShootingState.coroutineHelper = coroutineHelper;
        activeShootingState.transitions = new List<Transition>() { investigateTransition, relocateTransition };

        // sees player -> active shooting
        // reaches aprox last player position -> wait
        FoundPlayerTransition foundPlayerTransition = new FoundPlayerTransition(navigateFiringPosState, this);
        ReachedLastKnownPlayerPosTransition reachedLastKnownPlayerPosTransition = new ReachedLastKnownPlayerPosTransition(waitState, this, transform);
        investigatePlayerState.transitions = new List<Transition>() { foundPlayerTransition, reachedLastKnownPlayerPosTransition };

        // wait state
        // sees player -> navigate to shooting position
        // time passes -> patrol
        waitState.coroutineHelper = coroutineHelper;
        ExternalControlTransition waitToPatrol = new ExternalControlTransition(patrolState);
        waitState.transitions = new List<Transition>() { navigateFiring, waitToPatrol };

        CurrentState = patrolState;
        CurrentState.OnEntry();
    }

    public void SetPatrolPoints(Transform[] points)
    {
        List<Vector3> pointsList = new List<Vector3>();
        for (int i = 0; i < points.Length; i++)
        {
            pointsList.Add(points[i].position);
        }
        if (patrolState == null)
            patrolState = new PatrolState();
        patrolState.SetPatrolPoints(pointsList);
    }

    //idk if we'll need this at some point
    /*public void SetCoverPoints(Transform[] points)
    {
        List<Vector3> pointsList = new List<Vector3>();
        for (int i = 0; i < points.Length; i++)
        {
            pointsList.Add(points[i].position);
        }
        if (navigateFiringPosState == null)
            navigateFiringPosState = new NavigateFiringPosState();
        navigateFiringPosState.SetCoverPoints(pointsList);
    }*/

    private void Start()
    {
        patrolState.pingpong = patrolPingPong;
    }
}
