using UnityEngine;

[RequireComponent(typeof(SniperSweepState), typeof(InvestigatePlayerState)), RequireComponent(typeof(SniperAttackState), typeof(WaitState))]
public class SniperAI : AIController
{
	// AI States
	protected SniperSweepState sniperSweepState;
	//protected NavigateFiringPosState navigateFiringPosState;
	protected InvestigatePlayerState investigatePlayerState;
	protected SniperAttackState sniperAttackState;
	protected WaitState waitState;

	protected override void Awake()
	{
		base.Awake();

		sniperSweepState = GetComponent<SniperSweepState>();
		//navigateFiringPosState = GetComponent<NavigateFiringPosState>();
		//investigatePlayerState = GetComponent<InvestigatePlayerState>();
		sniperAttackState = GetComponent<SniperAttackState>();
		waitState = GetComponent<WaitState>();

		sniperSweepState.Setup(new CanSeePlayerTransition(sniperAttackState, this));
		//navigateFiringPosState.Setup(new ExternalControlTransition(patrolState), new StartShootingTransition(activeShootingState));
		//investigatePlayerState.Setup(new ReachedLastKnownPlayerPosTransition(waitState, this), new ExternalControlTransition(patrolState));
		sniperAttackState.Setup(new ExternalControlTransition(sniperSweepState));
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

