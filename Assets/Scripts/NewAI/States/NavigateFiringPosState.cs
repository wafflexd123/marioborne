using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigateFiringPosState : IAIState
{
    public List<Transition> transitions { get; set; }
    public AIController controller { get; set; }

    protected Vector3 targetLocation;

    public void OnEntry()
    {
        // Find a target location. 
        // Target location should be somewhere the AI can see the player
        // And somewhere they have some cover
        // Set targetLocation

        // according to Enemy.cs
        controller.agent.SetDestination(Player.singlePlayer.transform.position);
    }

    public void OnExit()
    {
        throw new System.NotImplementedException();
    }

    public void Tick()
    {
        // Move towards target location
        // according to Enemy.cs
        Vector3 velocity = Vector3.zero; // does this suffice ???
        controller.transform.position = Vector3.SmoothDamp(controller.transform.position, controller.agent.nextPosition, ref velocity, 0.1f);
    }
}

public class StartShootingTransition : Transition
{
    public Vector3 destination = Vector3.zero;
    public Vector3 position = Vector3.one;

    public override bool RequirementsMet()
    {
        return Vector3.Distance(destination, position) < 0.05f;
    }
}