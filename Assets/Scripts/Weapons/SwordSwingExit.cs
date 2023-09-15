using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordSwingExit : StateMachineBehaviour
{
    private Knife knife;

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        knife = knife == null ? animator.transform.GetComponent<Knife>() : knife;
        knife.DisableHitbox();
    }
}
