using UnityEngine;

[RequireComponent(typeof(PatrolState), typeof(InvestigatePlayerState)), RequireComponent(typeof(ShieldApproachState), typeof(WaitState))]
public class ShieldAI : AIController
{
    // AI States
    protected PatrolState patrolState;
    //protected NavigateFiringPosState navigateFiringPosState;
    protected InvestigatePlayerState investigatePlayerState;
    protected ShieldApproachState shieldApproachState;
    protected WaitState waitState;

    protected override void Awake()
    {
        base.Awake();

        patrolState = GetComponent<PatrolState>();
        //navigateFiringPosState = GetComponent<NavigateFiringPosState>();
        investigatePlayerState = GetComponent<InvestigatePlayerState>();
        shieldApproachState = GetComponent<ShieldApproachState>();
        waitState = GetComponent<WaitState>();

        patrolState.Setup(new CanSeePlayerTransition(shieldApproachState, this), new CanHearPlayerTransition(investigatePlayerState, this));
        //navigateFiringPosState.Setup(new ExternalControlTransition(patrolState), new StartShootingTransition(activeShootingState));
        investigatePlayerState.Setup(new FoundPlayerTransition(shieldApproachState, this), new ReachedLastKnownPlayerPosTransition(waitState, this), new ExternalControlTransition(patrolState));
        shieldApproachState.Setup(new ExternalControlTransition(investigatePlayerState));
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