using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletSoundEF : MonoBehaviour
{
    public GameObject audioReflecionSound;
    private void OnTriggerEnter(Collider other)
    {

     

            Instantiate(audioReflecionSound, transform.position, transform.rotation);

    }
}
