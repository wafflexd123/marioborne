using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviourPlus
{
    public enum EnemyType
    {
        Ranged,
        Melee
    }

    public Transform[] points;
    private int destPoint = 0;
    private NavMeshAgent agent;

    public EnemyType type;
    public GameObject player;
    public float sightRadius, fireDelay, bulletSpeed;
    public bool fireOnFirstFrame;
    public Bullet bulletPrefab;
    public Transform firePosition;
    float timer;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player");
        if (fireOnFirstFrame) timer = fireDelay;
    }

    // Update is called once per frame
    void Update()
    {
        Collider[] ray = Physics.OverlapSphere(transform.position, sightRadius, 1 << 3);
        timer += Time.deltaTime;
        if (ray.Length > 0 && ray[0] != null && FindComponent(ray[0].transform, out PlayerMovement player))
        {
            agent.isStopped = true;
            transform.LookAt(player.transform);
            if (timer > fireDelay)
            {
                timer = 0;

                Vector3 chest = player.head.position;
                chest.y /= 2;
                Instantiate(bulletPrefab, firePosition.position, Quaternion.identity).Initialise(bulletSpeed, (chest - firePosition.position).normalized);
            }
        }
        else agent.isStopped = false;
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            GoToNextPoint();
        }
    }

    void GoToNextPoint()
    {
        if (points.Length == 0) return;

        agent.destination = points[destPoint].position;

        destPoint = (destPoint + 1) % points.Length;
    }
}
