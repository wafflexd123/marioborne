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
    
    void Awake()
    {
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
    }

    private void Start()
    {
        patrolState.pingPong = patrolPingPong;
    }
}
