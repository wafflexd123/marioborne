using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeLive : MonoBehaviour
{
    [Header("Explosion")]
    [SerializeField] private float lifeTimeAfterGround = 1.3f;
    [SerializeField] public static float maxLifetime = 4f;
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private float explosionForce = 50f;
    [SerializeField] private float explosionUpwardsMod = 3f;
    [SerializeField] private List<Color> Colors = new List<Color>();
    [SerializeField] private float emissivePower = 5f;
    [SerializeField] private float frameDuration = 2f / 60f;
    [SerializeField] private ParticleSystem[] particlesPrefabs;
    private static ParticleSystem[] particleInstances = null;
    [SerializeField] private GameObject sphere;
    private bool hasHitGround = false;
    private bool exploding = false;
    private Material mat;
    private List<GameObject> hitObjects = new List<GameObject>();
    private AudioSource aus;

    void Start()
    {
        StartCoroutine(DetonateAfterTime());
        aus = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator Explode()
    {
        GameObject explosionObj = Instantiate(sphere, transform.position, Quaternion.identity);
        mat = explosionObj.GetComponent<MeshRenderer>().material;
        explosionObj.transform.localScale = Vector3.one * explosionRadius;
        GetComponent<MeshRenderer>().enabled = false;
        aus.Play();

        // collision and physics
        int layermaska = ~0 & ~(1 << 3);
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius, layermaska);
        for (int i = 0; i < hitColliders.Length; i++)
        {
            GameObject hitObj = hitColliders[i].gameObject;
            print("Explosion collided with: " + hitColliders[i].name + ", on layer: " + hitObj.layer);
            try
            {
                Humanoid humanoid = hitObj.GetComponentInParent<Humanoid>();
                humanoid.Kill();
                print("Grenade Killing humanoid: " + hitObj.name);
            }
            catch
            {
                /// *** THIS IS NOT WORKING ***
                Rigidbody hitrb;
                if (hitObj.TryGetComponent<Rigidbody>(out hitrb))
                {
                    print("found rb on same object as collider");
                }
                hitrb = hitrb == null ? hitObj.GetComponentInParent<Rigidbody>() : hitrb;
                if (hitrb != null)
                {
                    hitrb.AddExplosionForce(explosionForce, transform.position, explosionRadius, explosionUpwardsMod, ForceMode.Impulse);
                    print("Grenade Applying explosive force to: " + hitObj.name);
                }
            }
        }

        // particles
        PlaceAndPlayParticles(transform.position);

        // animation
        for (int frame = 0; frame < Colors.Count; frame++)
        {
            mat.SetColor("_EmissionColor", Colors[frame] * Mathf.LinearToGammaSpace(emissivePower));
            // set material color
            //Mathf.LinearToGammaSpace(2f)
            yield return new WaitForSecondsRealtime(frameDuration);
        }
        Destroy(explosionObj);
        Destroy(gameObject);
    }

    private void PlaceAndPlayParticles(Vector3 pos)
    {
        if (particleInstances == null)
            GenerateStaticParticles();
        for (int i = 0; i < particleInstances.Length; i++)
        {
            particleInstances[i].transform.position = pos;
            particleInstances[i].Play();
        }
    }

    private void GenerateStaticParticles()
    {
        particleInstances = new ParticleSystem[particlesPrefabs.Length];
        for (int i = 0; i < particleInstances.Length;i++)
            particleInstances[i] = Instantiate(particlesPrefabs[i], Vector3.zero, Quaternion.identity).GetComponent<ParticleSystem>();
    }

    private IEnumerator WaitToExplode()
    {
        yield return new WaitForSeconds(lifeTimeAfterGround);
        exploding = true;
        StartCoroutine(Explode());
    }

    private void OnCollisionEnter(Collision collision)
    {
        print("Live grenade collided with: " + collision.gameObject.name);
        if (!hasHitGround)
        {
            hasHitGround = true;
            StartCoroutine(WaitToExplode());
        }
    }

    private IEnumerator DetonateAfterTime()
    {
        yield return new WaitForSeconds(maxLifetime);
        if (!hasHitGround)
        {
            hasHitGround = true;
            StartCoroutine(Explode());
        }
    }
}
