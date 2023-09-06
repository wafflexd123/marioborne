using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateHelper : MonoBehaviour
{
    //public Transform[] points;
    public List<Vector3> patrolPoints;
    private int patrolIndex = 0;
    public AIController controller { get; set; }
    protected StandardAI standardAI;
    protected MeleeAI meleeAI;
    protected ShieldAI shieldAI;

    //public List<Vector3> SetPatrolRoute(PatrolState.EnemyType type)
    //{
    //    patrolPoints = new List<Vector3>();
    //    if (type == PatrolState.EnemyType.Standard)
    //    {
    //        standardAI = GetComponent<StandardAI>();
    //        for (int i = 0; i < standardAI.patrolPoints.childCount - 1; i++)
    //        {
    //            patrolPoints.Add(standardAI.patrolPoints.GetChild(i).position);
    //        }
    //        FindClosest(patrolPoints);
    //    }
    //    if (type == PatrolState.EnemyType.Melee)
    //    {
    //        meleeAI = GetComponent<MeleeAI>();
    //        for (int i = 0; i < meleeAI.patrolPoints.childCount - 1; i++)
    //        {
    //            patrolPoints.Add(meleeAI.patrolPoints.GetChild(i).position);
    //        }
    //        FindClosest(patrolPoints);
    //    }
    //    if (type == PatrolState.EnemyType.Shield)
    //    {
    //        shieldAI = GetComponent<ShieldAI>();
    //        for (int i = 0; i < shieldAI.patrolPoints.childCount - 1; i++)
    //        {
    //            patrolPoints.Add(shieldAI.patrolPoints.GetChild(i).position);
    //        }
    //        FindClosest(patrolPoints);
    //    }
    //    return patrolPoints;
    //}
    /*
    public int GetPatrolIndex()
    {
        return patrolIndex;
    }

    public int FindClosest(List<Vector3> points)
    {
        float closestDistance = float.MaxValue;
        for (int i = 0; i < points.Count; i++)
        {
            if (Vector3.Distance(controller.transform.position, points[i]) < closestDistance)
            {
                patrolIndex = i;
                closestDistance = Vector3.Distance(controller.transform.position, points[i]);
            }
        }
        return patrolIndex;
    }

    public float GetDefaultSpeed(AttackingState.EnemyType type)
    {
        if (type == AttackingState.EnemyType.Melee)
        {
            meleeAI = GetComponent<MeleeAI>();
            return meleeAI.defaultSpeed;
        }
        if (type == AttackingState.EnemyType.Shield)
        {
            shieldAI = GetComponent<ShieldAI>();
            return shieldAI.defaultSpeed;
        }
        return 1;
    }

    public float GetChaseSpeed(AttackingState.EnemyType type)
    {
        if (type == AttackingState.EnemyType.Melee)
        {
            meleeAI = GetComponent<MeleeAI>();
            return meleeAI.chaseSpeed;
        }
        if (type == AttackingState.EnemyType.Shield)
        {
            shieldAI = GetComponent<ShieldAI>();
            return shieldAI.chaseSpeed;
        }
        return 1;
    }

    public float GetMeleeDistance(AttackingState.EnemyType type)
    {
        if (type == AttackingState.EnemyType.Melee)
        {
            meleeAI = GetComponent<MeleeAI>();
            return meleeAI.meleeDistance;
        }
        if (type == AttackingState.EnemyType.Shield)
        {
            shieldAI = GetComponent<ShieldAI>();
            return shieldAI.meleeDistance;
        }
        return 1;
    }

    private void Awake()
    {

    }
    */
}
