using System;
using UnityEngine;

[RequireComponent(typeof(PatrolState), typeof(InvestigatePlayerState)), RequireComponent(typeof(ShieldAttackState), typeof(WaitState))]
public class ShieldAI : AIController
{
    // AI States
    protected PatrolState patrolState;
    //protected NavigateFiringPosState navigateFiringPosState;
    protected InvestigatePlayerState investigatePlayerState;
    protected ShieldAttackState shieldAttackState;
    protected WaitState waitState;

    protected override void Awake()
    {
        base.Awake();

        patrolState = GetComponent<PatrolState>();
        //navigateFiringPosState = GetComponent<NavigateFiringPosState>();
        investigatePlayerState = GetComponent<InvestigatePlayerState>();
        shieldAttackState = GetComponent<ShieldAttackState>();
        waitState = GetComponent<WaitState>();

        patrolState.Setup(new CanSeePlayerTransition(shieldAttackState, this), new CanHearPlayerTransition(investigatePlayerState, this));
        //navigateFiringPosState.Setup(new ExternalControlTransition(patrolState), new StartShootingTransition(activeShootingState));
        investigatePlayerState.Setup(new FoundPlayerTransition(shieldAttackState, this), new ReachedLastKnownPlayerPosTransition(waitState, this), new ExternalControlTransition(patrolState));
        shieldAttackState.Setup(new ExternalControlTransition(investigatePlayerState));
        waitState.Setup(new ExternalControlTransition(patrolState));

        CurrentState = patrolState.BeginState();

        // reach destination -> active shooting
        // 10s pass -> patrol
        // sees player -> active shooting
        // reaches aprox last player position -> wait
        // wait state
        // sees player -> navigate to shooting position
        // time passes -> patrol
    }
}