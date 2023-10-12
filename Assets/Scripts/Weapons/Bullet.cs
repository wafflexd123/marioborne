using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(BasicRewindable))]
public class Bullet : MonoBehaviourPlus, IRewindListener
{
    //Inspector
    public float maxLifetime;

    //Script
    [HideInInspector] public float speed;
    [HideInInspector] public MonoBehaviour shooter;
    [HideInInspector] public bool penetrates;
    float timer;
    //new ParticleSystem particleSystem;
    new Renderer renderer;
    new Rigidbody rigidbody;
    new Collider collider;
    Vector3 _direction;

    public Vector3 direction
    {
        get => _direction;
        set
        {
            _direction = value;
            transform.rotation = Quaternion.LookRotation(_direction);
        }
    }

    public Color color
    {
        get => renderer.material.color;
        set
        {
            //ParticleSystem.MainModule p = GetComponent<ParticleSystem>().main;
            //p.startColor = value;
            renderer.material.color = value;
        }
    }

    public virtual Bullet Initialise(float speed, Vector3 direction, Humanoid shooter, Color color, bool penetrates)
    {
        this.speed = speed;
        this.direction = direction;
        this.shooter = shooter;
        this.color = color;
        this.penetrates = penetrates;
        enabled = true;
        return this;
    }

    protected virtual void Awake()
    {
        //particleSystem = transform.Find("Trail").GetComponent<ParticleSystem>();
        renderer = transform.Find("Model").GetComponent<Renderer>();
        collider = renderer.GetComponent<Collider>();
        rigidbody = GetComponent<Rigidbody>();
    }

    protected virtual void FixedUpdate()
    {
        rigidbody.velocity = Time.timeScale * speed * direction;
        timer += Time.fixedDeltaTime;
        if (timer >= maxLifetime) Destroy(gameObject);
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (FindComponent(collision.transform, out IBulletReceiver bulletReceiver))
        {
            bulletReceiver.OnBulletHit(collision, this);
        }
        else Destroy(gameObject);
    }

    protected virtual void OnEnable()
    {
        rigidbody.isKinematic = false;
        collider.enabled = true;
        //particleSystem.Play();
    }

    protected virtual void OnDisable()
    {
        rigidbody.isKinematic = true;
        collider.enabled = false;
        //particleSystem.Stop();
    }

    public void Rewind(float seconds)
    {
        
    }

    public void StartRewind()
    {
        direction *= -1;
    }

    public void StopRewind()
    {
        direction *= -1;
    }
}

public interface IBulletReceiver
{
    public void OnBulletHit(Collision collision, Bullet bullet);
}
