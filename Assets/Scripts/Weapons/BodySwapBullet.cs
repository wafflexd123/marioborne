using UnityEngine;

public class BodySwapBullet : MonoBehaviourPlus
{
    public float teleportSpeed, maxTeleportTime;
    [SerializeField] private Vector3 throwForce;
    [SerializeField] private float daggerGravity;
    private Vector3 gravityForce = Vector3.zero;
    //[SerializeField] private float daggerSpeed;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        gravityForce = Vector3.up * -daggerGravity;
    }

    public void Fire()
    {
        //throwForce = Player.singlePlayer.transform.TransformDirection(throwForce);
        throwForce = Player.singlePlayer.camera.transform.TransformDirection(throwForce);
        rb = GetComponent<Rigidbody>();
        rb.AddForce(throwForce, ForceMode.Impulse);
    }

    private void FixedUpdate()
    {
        rb.AddForce(gravityForce, ForceMode.Acceleration);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<Humanoid>(out Humanoid man))
        {
            Player.singlePlayer.TeleportToEnemy(man, teleportSpeed, maxTeleportTime);
        }
        Destroy(gameObject);
    }

    //protected override void OnCollisionEnter(Collision collision)
    //{
    //    if (FindComponent(collision.transform, out Humanoid enemy) && enemy != shooter)
    //    {
    //        ((Player)shooter).TeleportToEnemy(enemy, teleportSpeed, maxTeleportTime);
    //    }
    //    Destroy(gameObject);
    //}
}
