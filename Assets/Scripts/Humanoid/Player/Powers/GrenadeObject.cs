using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeObject : MonoBehaviour
{
    [SerializeField] private float lifeTimeAfterGround = 1.3f;
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private List<Color> Colors = new List<Color>();
    [SerializeField] private ParticleSystem smokeParticles;
    private bool hasHitGround = false;
    private bool exploding = false;
    private Material mat;
    private List<GameObject> hitObjects = new List<GameObject>();

    void Start()
    {
        mat = GetComponent<Material>();
    }

    // Update is called once per frame
    void Update()
    {
        
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
}
