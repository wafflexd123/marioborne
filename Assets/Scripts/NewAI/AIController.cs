using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(FieldOfView), typeof(Rigidbody))]
public class AIController : Humanoid, ITimeScaleListener
{
    protected List<IAIState> states = new List<IAIState>();
    public IAIState CurrentState { get; protected set; }
    public Vector3 LastKnownPlayerPosition { get; protected set; }
    public FieldOfView fieldOfView { get; protected set; }
    public NavMeshAgent agent { get; protected set; }
    public Rigidbody rb { get; protected set; }
    Vector3 lookingAt;

    public override Vector3 LookDirection => fieldOfView.eyes.transform.TransformDirection(Vector3.forward);
    public override Vector3 LookingAt => lookingAt;

    [SerializeField] protected WeaponBase weapon;
    [SerializeField] public Transform patrolPoints;
    protected Player player;

    protected Vector3 velocity;

    protected override void Awake()
    {
        base.Awake();
        fieldOfView = GetComponent<FieldOfView>();
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        player = Player.singlePlayer;
        Time.timeScaleListeners.Add(this);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        MoveTowards(GameObject.Find("Player").transform.position); //for debugging
        Fire(); //for debugging
        bool TransitionedThisFrame = false;
        /*for (int i = 0; i < CurrentState.transitions.Count; i++)
        {
            if (CurrentState.transitions[i].RequirementsMet()) // transition state
            {
                CurrentState.OnExit();
                CurrentState = CurrentState.transitions[i].targetState;
                CurrentState.OnEntry();
                TransitionedThisFrame = true;
                break;
            }
        }
        if (!TransitionedThisFrame)
        {
            CurrentState.Tick();
        }
        */
    }

    public void MoveTowards(Vector3 targetPosition) 
    {
        velocity = agent.velocity;
        transform.position = Vector3.SmoothDamp(transform.position, agent.nextPosition, ref velocity, 0.1f);
        agent.SetDestination(targetPosition);
    }

    public void Fire()
    {
        // TO DO
        lookingAt = player.camera.transform.position;
        weapon.transform.LookAt(lookingAt);
        input.Press("Mouse", () => -1, () => false);
    }

    public void OnTimeSlow()
    {
        agent.speed = agent.speed * Time.timeScale;
    }

    public override void Kill(DeathType deathType = DeathType.General)
    {
        throw new NotImplementedException();
    }

    public override bool PickupObject(WeaponBase weapon, out Action onDrop)
    {
        if (!this.weapon)
        {
            this.weapon = weapon;
            //typeOfWeapon = weapon is Gun ? EnemyType.Ranged : EnemyType.Melee;
            onDrop = () => this.weapon = null;
            return true;
        }
        onDrop = null;
        return false;
    }
}
