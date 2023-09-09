using UnityEngine;

[RequireComponent(typeof(PatrolState), typeof(InvestigatePlayerState)), RequireComponent(typeof(SnipingState), typeof(WaitState))]
public class SniperAI : AIController
{
	// AI States
	protected PatrolState patrolState;
	//protected NavigateFiringPosState navigateFiringPosState;
	protected InvestigatePlayerState investigatePlayerState;
	protected SnipingState snipingState;
	protected WaitState waitState;

	protected override void Awake()
	{
		base.Awake();

		patrolState = GetComponent<PatrolState>();
		//navigateFiringPosState = GetComponent<NavigateFiringPosState>();
		//investigatePlayerState = GetComponent<InvestigatePlayerState>();
		snipingState = GetComponent<SnipingState>();
		waitState = GetComponent<WaitState>();

		patrolState.Setup(new CanSeePlayerTransition(snipingState, this));
		//navigateFiringPosState.Setup(new ExternalControlTransition(patrolState), new StartShootingTransition(activeShootingState));
		//investigatePlayerState.Setup(new ReachedLastKnownPlayerPosTransition(waitState, this), new ExternalControlTransition(patrolState));
		snipingState.Setup(new ExternalControlTransition(patrolState));
		//waitState.Setup(new ExternalControlTransition(patrolState));

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

