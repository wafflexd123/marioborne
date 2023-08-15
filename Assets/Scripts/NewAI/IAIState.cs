using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAIState
{
    void Tick();
    void OnEntry();
    void OnExit();
    //void Setup();

    public List<Transition> transitions { get; set; }
    public AIController controller { get; set; }
}
//public abstract class IAIState

public abstract class Transition
{
    public IAIState targetState { get; private set; }
    public virtual bool RequirementsMet() { return false; }
}
