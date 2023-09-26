using UnityEngine;

[RequireComponent(typeof(SniperSweepState), typeof(InvestigatePlayerState)), RequireComponent(typeof(SnipingState), typeof(WaitState))]
public class SniperAI : AIController
{
	// AI States
	protected SniperSweepState sniperSweepState;
	//protected NavigateFiringPosState navigateFiringPosState;
	protected InvestigatePlayerState investigatePlayerState;
	protected SnipingState snipingState;
	protected WaitState waitState;

	protected override void Awake()
	{
		base.Awake();

		sniperSweepState = GetComponent<SniperSweepState>();
		//navigateFiringPosState = GetComponent<NavigateFiringPosState>();
		//investigatePlayerState = GetComponent<InvestigatePlayerState>();
		snipingState = GetComponent<SnipingState>();
		waitState = GetComponent<WaitState>();

		sniperSweepState.Setup(new CanSeePlayerTransition(snipingState, this));
		//navigateFiringPosState.Setup(new ExternalControlTransition(patrolState), new StartShootingTransition(activeShootingState));
		//investigatePlayerState.Setup(new ReachedLastKnownPlayerPosTransition(waitState, this), new ExternalControlTransition(patrolState));
		snipingState.Setup(new ExternalControlTransition(sniperSweepState));
		//waitState.Setup(new ExternalControlTransition(patrolState));

		CurrentState = sniperSweepState.BeginState();

		// reach destination -> active shooting
		// 10s pass -> patrol
		// sees player -> active shooting
		// reaches aprox last player position -> wait
		// wait state
		// sees player -> navigate to shooting position
		// time passes -> patrol
	}
}

