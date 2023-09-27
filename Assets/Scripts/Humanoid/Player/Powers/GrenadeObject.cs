using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeObject : MonoBehaviour
{
    [Header("Explosion")]
    [SerializeField] private float lifeTimeAfterGround = 1.3f;
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private List<Color> Colors = new List<Color>();
    [SerializeField] private ParticleSystem smokeParticles;
    private bool hasHitGround = false;
    private bool exploding = false;
    private Material mat;
    private List<GameObject> hitObjects = new List<GameObject>();

    [Header("In Hand")]
    [SerializeField] private float rotationSpeed;
    private Quaternion localRot;
    private Quaternion frameRotation;
    [SerializeField] private float amplitude;
    [SerializeField] private float period;
    private LazyFollower lazyFollower;

    void Start()
    {
        mat = GetComponent<Material>();
        lazyFollower = GetComponent<LazyFollower>();
        if (lazyFollower != null)
            lazyFollower.offsetFunc = new LazyFollower.offsetDelegate(FrameOffset);
        else Debug.LogWarning("Grenade could not find lazy follower, please be upset");

        localRot = transform.localRotation;
    }

    private Vector3 FrameOffset()
    {
        return Vector3.up * (amplitude * Mathf.Sin(UnityEngine.Time.time * period));
    }

    void Update()
    {
        frameRotation = Quaternion.Euler(0f, rotationSpeed * Time.deltaTime, 0f);
        localRot *= frameRotation;
        transform.localRotation = localRot;
    }

    private IEnumerator Explode()
    {
        for (int frame = 0; frame < Colors.Count; frame++)
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);
            // set material color
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator WaitToExplode()
    {
        yield return new WaitForSeconds(lifeTimeAfterGround);
        exploding = true;
        StartCoroutine(Explode());
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!hasHitGround)
        {
            hasHitGround = true;
            StartCoroutine(WaitToExplode());
        }
    }

    private void OnEnable() { if (lazyFollower != null) lazyFollower.enabled = true; }
    private void OnDisable() { if (lazyFollower != null) lazyFollower.enabled = false; }
}
