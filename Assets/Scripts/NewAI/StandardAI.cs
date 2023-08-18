using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.VersionControl.Asset;

public class StandardAI : AIController
{
    [SerializeField, Tooltip("Whether the patrol path should be run in reverse when completed, or looping from the 0th patrol point")] 
    protected bool patrolPingPong = true;
    
    // AI States
    protected PatrolState patrolState;
    protected NavigateFiringPosState navigateFiringPosState;
    protected InvestigatePlayerState investigatePlayerState;
    protected ActiveShootingState activeShootingState;
    protected WaitState waitState;
    
    protected override void Awake()
    {
        base.Awake();
        var coroutineHelper = gameObject.AddComponent<CoroutineHelper>();
        states = new List<IAIState>();
        patrolState = new PatrolState();
        states.Add(patrolState);
        navigateFiringPosState = new NavigateFiringPosState();
        states.Add(navigateFiringPosState);
        investigatePlayerState = new InvestigatePlayerState();
        states.Add(investigatePlayerState);
        activeShootingState = new ActiveShootingState();
        states.Add(activeShootingState);
        waitState = new WaitState();
        states.Add(waitState);

        CanSeePlayerTransition NavigateFiring = new CanSeePlayerTransition(navigateFiringPosState, this);
        patrolState.transitions = new List<Transition>() { NavigateFiring };

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
        ReachedLastKnownPlayerPosTransition reachedLastKnownPlayerPosTransition = new ReachedLastKnownPlayerPosTransition(waitState, this, transform);
        investigatePlayerState.transitions = new List<Transition>() { startShootingTransition, reachedLastKnownPlayerPosTransition };

        // wait state
        // sees player -> navigate to shooting position
        // time passes -> patrol
        waitState.coroutineHelper = coroutineHelper;
        ExternalControlTransition waitToPatrol = new ExternalControlTransition(patrolState);
        waitState.transitions = new List<Transition>() { startShootingTransition, waitToPatrol };
    }

    private void Start()
    {
        patrolState.pingPong = patrolPingPong;
    }
}
