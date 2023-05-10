using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AfterCarparkLevelManager : MonoBehaviour
{
    public TriggerCollider firstEnemyTrigger;
    public EnemyPath[] enemySpawns;

    IEnumerator Start()
    {
        yield return new WaitUntil(() => firstEnemyTrigger.isTriggered);
        enemySpawns[0].SetPaths();
    }
}
