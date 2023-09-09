using UnityEngine;

[RequireComponent(typeof(PatrolState), typeof(InvestigatePlayerState)), RequireComponent(typeof(AttackingState), typeof(WaitState))]
public class ShieldAI : AIController
{
    // AI States
    protected PatrolState patrolState;
    //protected NavigateFiringPosState navigateFiringPosState;
    protected InvestigatePlayerState investigatePlayerState;
    protected AttackingState attackingState;
    protected WaitState waitState;

    protected override void Awake()
    {
        base.Awake();

        patrolState = GetComponent<PatrolState>();
        //navigateFiringPosState = GetComponent<NavigateFiringPosState>();
        investigatePlayerState = GetComponent<InvestigatePlayerState>();
        attackingState = GetComponent<AttackingState>();
        waitState = GetComponent<WaitState>();

        patrolState.Setup(new CanSeePlayerTransition(attackingState, this), new CanHearPlayerTransition(investigatePlayerState, this));
        //navigateFiringPosState.Setup(new ExternalControlTransition(patrolState), new StartShootingTransition(activeShootingState));
        investigatePlayerState.Setup(new FoundPlayerTransition(attackingState, this), new ReachedLastKnownPlayerPosTransition(waitState, this), new ExternalControlTransition(patrolState));
        attackingState.Setup(new ExternalControlTransition(investigatePlayerState));
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