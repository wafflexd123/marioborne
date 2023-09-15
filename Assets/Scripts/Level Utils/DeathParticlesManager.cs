using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathParticlesManager : MonoBehaviour
{
    public static DeathParticlesManager Current { get; private set; }
    [SerializeField] private ParticleSystem deathParticles;
    private List<ParticleSystem> particlePool;

    [SerializeField] private int initPoolSize = 3;
    private int curIndex = 0;

    void Start()
    {
        particlePool = new List<ParticleSystem>();
        for (int i = 0; i < initPoolSize; i++)
        {
            ParticleSystem newSystem = Instantiate(deathParticles, Vector3.zero, Quaternion.identity);
            particlePool.Add(newSystem);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayAtLocation(Vector3 location)
    {
        ParticleSystem particle = particlePool[curIndex];
        particle.gameObject.transform.position = location;
        particle.Play();
        IncrementIndex();
    }

    private void IncrementIndex()
    {
        curIndex = (curIndex + 1) % particlePool.Count;
    }

    private void Awake()
    {
        Current = this;
    }
}
