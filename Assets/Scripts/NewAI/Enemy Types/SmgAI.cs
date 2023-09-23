using UnityEngine;

[RequireComponent(typeof(PatrolState), typeof(InvestigatePlayerState)), RequireComponent(typeof(SmgAttackState), typeof(WaitState))]
public class SmgAI : AIController
{
	// AI States
	protected PatrolState patrolState;
	protected InvestigatePlayerState investigatePlayerState;
	protected SmgAttackState smgAttackState;
	protected WaitState waitState;

	protected override void Awake()
	{
		base.Awake();

		patrolState = GetComponent<PatrolState>();
		investigatePlayerState = GetComponent<InvestigatePlayerState>();
		smgAttackState = GetComponent<SmgAttackState>();
		waitState = GetComponent<WaitState>();

		patrolState.Setup(new CanSeePlayerTransition(smgAttackState, this), new CanHearPlayerTransition(investigatePlayerState, this));
		investigatePlayerState.Setup(new FoundPlayerTransition(smgAttackState, this), new ReachedLastKnownPlayerPosTransition(waitState, this), new ExternalControlTransition(patrolState));
		smgAttackState.Setup(new ExternalControlTransition(investigatePlayerState));
		waitState.Setup(new FoundPlayerTransition(smgAttackState, this), new ExternalControlTransition(patrolState));

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

