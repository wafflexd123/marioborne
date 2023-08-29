using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(FieldOfView), typeof(Rigidbody))]
public class AIController : Humanoid, ITimeScaleListener
{
    protected List<IAIState> states = new List<IAIState>();
    public IAIState CurrentState { get; protected set; }
    public Vector3 LastKnownPlayerPosition { get; set; }
    public FieldOfView fieldOfView { get; protected set; }
    public NavMeshAgent agent { get; protected set; }
    public Rigidbody rb { get; protected set; }
    [HideInInspector] public Vector3 lookingAt;
    [HideInInspector] public Player player;
    [HideInInspector] public Transform soundLocation;
    
    public override Vector3 LookDirection => fieldOfView.eyes.transform.TransformDirection(Vector3.forward);
    public override Vector3 LookingAt => lookingAt;

    [SerializeField] public WeaponBase weapon;
    [SerializeField] public float alertRadius;

    protected Vector3 velocity;

    public string stateName;
    private float timeEnteredState = 0f;

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
        model.velocity = agent.velocity;
        //MoveTowards(GameObject.Find("Player").transform.position); //for debugging
        //Fire(); //for debugging
        bool TransitionedThisFrame = false;
        for (int i = 0; i < CurrentState.transitions.Count; i++)
        {
            if (CurrentState.transitions[i].RequirementsMet()) // transition state
            {
                print($"{name} is transitioning: \t{i}: {CurrentState.GetType().Name} -> {CurrentState.transitions[i].targetState.GetType().Name}, \t time in state: {UnityEngine.Time.time - timeEnteredState}");
                CurrentState.OnExit(); 
                CurrentState = CurrentState.transitions[i].targetState;
                CurrentState.OnEntry();
                stateName = CurrentState.GetType().Name;
                TransitionedThisFrame = true;
                timeEnteredState = UnityEngine.Time.time;
                break;
            }
        }
        if (!TransitionedThisFrame)
        {
            CurrentState.Tick();
        }
    }

    public void MoveTowards(Vector3 targetPosition) 
    {
        velocity = agent.velocity;
        transform.position = Vector3.SmoothDamp(transform.position, agent.nextPosition, ref velocity, 0.1f);
        agent.SetDestination(targetPosition);
    }

    public void Fire()
    {
        lookingAt = player.camera.transform.position;
        if (weapon)
        {
            weapon.transform.LookAt(lookingAt);
            input.Press("Attack", () => -1, () => false);
        }
    }

    public void AlertOthers()
    {
        Collider[] alertOthers = Physics.OverlapSphere(transform.position, alertRadius);
        foreach (var alerted in alertOthers)
        { //creates an overlap sphere around enemy, checks if other enemies are in it and prompts them to investigate
            if (alerted.gameObject.layer.Equals(LayerMask.NameToLayer("Enemy")) && Vector3.Distance(alerted.transform.position, transform.position) > 1f)
            {
                alerted.GetComponentInParent<AIController>().soundLocation = transform;
            }
        }
    }

    public void OnTimeSlow()
    {
        agent.speed = agent.speed * Time.timeScale;
    }

    public override void Kill(DeathType deathType = DeathType.General)
    {
        if (weapon) input.Press("Drop");//drop weapon if holding one
        Destroy(gameObject);
    }

    public override bool PickupObject(WeaponBase weapon, out Action onDrop)
    {
        if (!this.weapon)
        {
            this.weapon = weapon;
            onDrop = () => this.weapon = null;
            return true;
        }
        onDrop = null;
        return false;
    }
}
