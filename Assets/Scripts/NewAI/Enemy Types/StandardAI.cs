using UnityEngine;

[RequireComponent(typeof(PatrolState), typeof(NavigateFiringPosState), typeof(InvestigatePlayerState)), RequireComponent(typeof(ActiveShootingState), typeof(WaitState))]
public class StandardAI : AIController
{
	// AI States
	protected PatrolState patrolState;
	protected NavigateFiringPosState navigateFiringPosState;
	protected InvestigatePlayerState investigatePlayerState;
	protected ActiveShootingState activeShootingState;
	protected WaitState waitState;

	protected override void Awake()
	{
		base.Awake();

		patrolState = GetComponent<PatrolState>();
		navigateFiringPosState = GetComponent<NavigateFiringPosState>();
		investigatePlayerState = GetComponent<InvestigatePlayerState>();
		activeShootingState = GetComponent<ActiveShootingState>();
		waitState = GetComponent<WaitState>();

		patrolState.Setup(new CanSeePlayerTransition(navigateFiringPosState, this), new CanHearPlayerTransition(investigatePlayerState, this));
		navigateFiringPosState.Setup(new ExternalControlTransition(patrolState), new StartShootingTransition(activeShootingState));
		investigatePlayerState.Setup(new FoundPlayerTransition(navigateFiringPosState, this), new ReachedLastKnownPlayerPosTransition(waitState, this), new ExternalControlTransition(patrolState));
		activeShootingState.Setup(new ExternalControlTransition(investigatePlayerState), new ExternalControlTransition(navigateFiringPosState));
		waitState.Setup(new FoundPlayerTransition(navigateFiringPosState, this), new ExternalControlTransition(patrolState));

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

