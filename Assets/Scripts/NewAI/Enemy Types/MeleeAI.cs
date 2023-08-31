using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.VersionControl.Asset;

public class MeleeAI : AIController
{
    [SerializeField, Tooltip("Whether the patrol path should be run in reverse when completed, or looping from the 0th patrol point")]
    protected bool patrolPingPong = true;
    [SerializeField] public Transform patrolPoints;
    public float defaultSpeed;
    public float chaseSpeed;
    public float meleeDistance;

    // AI States
    protected PatrolState patrolState;
    //protected NavigateFiringPosState navigateFiringPosState;
    protected InvestigatePlayerState investigatePlayerState;
    protected AttackingState attackingState;
    protected WaitState waitState;

    // helpers
    protected CoroutineHelper coroutineHelper;
    //protected PatrolSetup patrolSetup;

    protected override void Awake()
    {
        base.Awake();
        try { coroutineHelper = gameObject.GetComponent<CoroutineHelper>(); }
        catch { coroutineHelper = gameObject.AddComponent<CoroutineHelper>(); }
        //try { patrolSetup = gameObject.GetComponent<PatrolSetup>(); }
        //catch { patrolSetup = gameObject.AddComponent<PatrolSetup>(); }
        stateHelper.controller = this;
        states = new List<IAIState>();
        patrolState ??= new PatrolState();
        patrolState.controller = this;
        patrolState.type = PatrolState.EnemyType.Melee;
        states.Add(patrolState);
        //navigateFiringPosState = new NavigateFiringPosState();
        //navigateFiringPosState.controller = this;
        //states.Add(navigateFiringPosState);
        investigatePlayerState = new InvestigatePlayerState();
        investigatePlayerState.controller = this;
        states.Add(investigatePlayerState);
        attackingState = new AttackingState();
        attackingState.controller = this;
        attackingState.type = AttackingState.EnemyType.Melee;
        states.Add(attackingState);
        waitState = new WaitState();
        waitState.controller = this;
        states.Add(waitState);

        CanSeePlayerTransition attacking = new CanSeePlayerTransition(attackingState, this);
        CanHearPlayerTransition investigateSound = new CanHearPlayerTransition(investigatePlayerState, this);
        patrolState.transitions = new List<Transition>() { attacking, investigateSound };
        // reach destination -> active shooting
        //AttackingTransition attackingTransition = new AttackingTransition(attackingState);
        //navigateFiringPosState.startShootingTransition = startShootingTransition;
        // 10s pass -> patrol
        //ExternalControlTransition returnToPatrolNavi = new ExternalControlTransition(patrolState);
        //navigateFiringPosState.coroutineHelper = coroutineHelper;
        //coroutineHelper.AddTimer("returnToPatrol");
        //navigateFiringPosState.returnToPatrolTransition = returnToPatrolNavi;
        //navigateFiringPosState.transitions = new List<Transition>() { returnToPatrolNavi, startShootingTransition };

        InvestigateTransition investigateTransition = new InvestigateTransition(investigatePlayerState);
        attackingState.coroutineHelper = coroutineHelper;
        attackingState.transitions = new List<Transition>() { investigateTransition };

        // sees player -> active shooting
        // reaches aprox last player position -> wait
        FoundPlayerTransition foundPlayerTransition = new FoundPlayerTransition(attackingState, this);
        ReachedLastKnownPlayerPosTransition reachedLastKnownPlayerPosTransition = new ReachedLastKnownPlayerPosTransition(waitState, this, transform);
        investigatePlayerState.coroutineHelper = coroutineHelper;
        InvestigateTransition investigateTimeout = new InvestigateTransition(patrolState);
        investigatePlayerState.transitions = new List<Transition>() { foundPlayerTransition, reachedLastKnownPlayerPosTransition, investigateTimeout };

        // wait state
        // sees player -> navigate to shooting position
        // time passes -> patrol
        waitState.coroutineHelper = coroutineHelper;
        ExternalControlTransition waitToPatrol = new ExternalControlTransition(patrolState);
        waitState.transitions = new List<Transition>() { attacking, waitToPatrol };

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