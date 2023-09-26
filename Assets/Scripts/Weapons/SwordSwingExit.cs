using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordSwingExit : StateMachineBehaviour
{
    private Sword sword;

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        sword = sword == null ? animator.transform.GetComponent<Sword>() : sword;
        sword.DisableHitbox();
    }
}