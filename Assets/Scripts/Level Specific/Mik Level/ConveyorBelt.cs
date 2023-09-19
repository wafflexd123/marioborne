using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConveyorBelt : MonoBehaviour
{
    [SerializeField] public float speed;
    [SerializeField] public float conveyorSpeed;

    [HideInInspector] public Vector3 direction;
    [HideInInspector] public PlayerMovement player;

    private List<GameObject> onBelt = new List<GameObject>();
    private Material material;

    // Start is called before the first frame update
    void Start()
    {
        direction = transform.forward;
        material = GetComponent<MeshRenderer>().material;
    }

    void Update()
    {
        material.mainTextureOffset -= new Vector2(0, 1) * conveyorSpeed * Time.deltaTime;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        foreach (GameObject obj in onBelt)
        {
            obj.GetComponentInChildren<Rigidbody>().AddForce(speed * direction, ForceMode.VelocityChange);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        onBelt.Add(collision.gameObject);
        if (collision.gameObject.GetComponentInChildren<PlayerMovement>())
        {
            player = collision.gameObject.GetComponentInChildren<PlayerMovement>();
            player.onConveyorBelt = this;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (player)
        {
            player.onConveyorBelt = null;
        }
        onBelt.Remove(collision.gameObject);
    }
}
