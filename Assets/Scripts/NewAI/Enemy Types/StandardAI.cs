using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PatrolState), typeof(NavigateFiringPosState), typeof(InvestigatePlayerState)), RequireComponent(typeof(ActiveShootingState), typeof(WaitState))]
public class StandardAI : AIController
{
	[Header("Patrol")]
	public Transform patrolPoints;

	// AI States
	protected PatrolState patrolState;
	protected NavigateFiringPosState navigateFiringPosState;
	protected InvestigatePlayerState investigatePlayerState;
	protected ActiveShootingState activeShootingState;
	protected WaitState waitState;

	protected override void Awake()
	{
		base.Awake();
		states = new List<IAIState>();
		states.Add(patrolState = GetComponent<PatrolState>());
		states.Add(navigateFiringPosState = GetComponent<NavigateFiringPosState>());
		states.Add(investigatePlayerState = GetComponent<InvestigatePlayerState>());
		states.Add(activeShootingState = GetComponent<ActiveShootingState>());
		states.Add(waitState = GetComponent<WaitState>());

		CanSeePlayerTransition navigateFiring = new CanSeePlayerTransition(navigateFiringPosState, this);
		CanHearPlayerTransition investigateSound = new CanHearPlayerTransition(investigatePlayerState, this);
		patrolState.transitions = new List<Transition>() { navigateFiring, investigateSound };
		// reach destination -> active shooting
		StartShootingTransition startShootingTransition = new StartShootingTransition(activeShootingState);
		navigateFiringPosState.startShootingTransition = startShootingTransition;
		// 10s pass -> patrol
		ExternalControlTransition returnToPatrolNavi = new ExternalControlTransition(patrolState);
		//navigateFiringPosState.coroutineHelper = coroutineHelper;
		//coroutineHelper.AddTimer("returnToPatrol");
		navigateFiringPosState.returnToPatrolTransition = returnToPatrolNavi;
		navigateFiringPosState.transitions = new List<Transition>() { returnToPatrolNavi, startShootingTransition };

		ExternalControlTransition investigateTransition = new ExternalControlTransition(investigatePlayerState);
		ExternalControlTransition relocateTransition = new ExternalControlTransition(navigateFiringPosState);
		//activeShootingState.coroutineHelper = coroutineHelper;
		activeShootingState.transitions = new List<Transition>() { investigateTransition, relocateTransition };

		// sees player -> active shooting
		// reaches aprox last player position -> wait
		FoundPlayerTransition foundPlayerTransition = new FoundPlayerTransition(navigateFiringPosState, this);
		ReachedLastKnownPlayerPosTransition reachedLastKnownPlayerPosTransition = new ReachedLastKnownPlayerPosTransition(waitState, this, transform);
		//investigatePlayerState.coroutineHelper = coroutineHelper;
		ExternalControlTransition investigateTimeout = new ExternalControlTransition(patrolState);
		investigatePlayerState.transitions = new List<Transition>() { foundPlayerTransition, reachedLastKnownPlayerPosTransition, investigateTimeout };

		// wait state
		// sees player -> navigate to shooting position
		// time passes -> patrol
		//waitState.coroutineHelper = coroutineHelper;
		ExternalControlTransition waitToPatrol = new ExternalControlTransition(patrolState);
		waitState.transitions = new List<Transition>() { navigateFiring, waitToPatrol };

		CurrentState = patrolState;
		CurrentState.OnEntry();
	}
}

