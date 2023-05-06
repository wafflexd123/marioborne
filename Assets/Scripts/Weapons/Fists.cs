using System.Collections;
using UnityEngine;

public class Fists : MonoBehaviourPlus
{
    public float hitDelay;
    public float deflectDelay;
    Coroutine crtPunchDelay;
    Coroutine crtDeflectDelay;
    public GameObject reflectWindow;
    private Player player;
    private new Rigidbody rigidbody;
    protected RigidbodyStore rigidbodyStore;

    private void Awake()
    {
        player = GetComponentInParent<Player>();
    }

    protected void Update()
    {
        if(player.hand.childCount > 0)
        {
            Enable(false);
        }
        else if (FindComponent(player.raycast.transform, out WeaponBase weapon))
        {
            if (Vector3.Distance(player.transform.position, weapon.transform.position) >= player.maxInteractDistance)
            {
                if (Input.GetMouseButtonDown(0)) LeftMouse();
                if (Input.GetMouseButtonDown(1)) RightMouse();
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0)) LeftMouse();
            if (Input.GetMouseButtonDown(1)) RightMouse();
        }


    }

    protected void LeftMouse()
    {
        Enable(true);

        if (crtPunchDelay == null && player.LookingAt != Vector3.negativeInfinity)//if not waiting for fireDelay && wielder is looking at something
        {
            player.model.attacking = true;
            crtPunchDelay = StartCoroutine(Delay());
        }

        IEnumerator Delay()
        {
            yield return new WaitForSeconds(hitDelay);
            Enable(false);
            player.model.attacking = false;
            crtPunchDelay = null;
        }
    }

    protected void RightMouse()
    {
        if (crtPunchDelay == null)
        {
            if (crtDeflectDelay == null && player.LookingAt != Vector3.negativeInfinity)
            {
                player.model.deflect = true;
                crtDeflectDelay = StartCoroutine(Delay());
            }

            IEnumerator Delay()
            {
                reflectWindow.SetActive(true);
                yield return new WaitForSeconds(deflectDelay);
                reflectWindow.SetActive(false);
                player.model.deflect = false;
                crtDeflectDelay = null;
            }
        }
    }

    private void Enable(bool enable)
    {
        if (enable)
        {
            if ((rigidbody = GetComponent<Rigidbody>()) == null) rigidbody = gameObject.AddComponent<Rigidbody>();//if no rigidbody exists yet
            if (rigidbodyStore != null) rigidbodyStore.Apply(rigidbody);
            GetComponent<Collider>().isTrigger = false;
        }
        else if(rigidbody != null)
        {
            rigidbodyStore = new RigidbodyStore(rigidbody);//store rigidbody data
            Destroy(rigidbody);
            GetComponent<Collider>().isTrigger = true;
        }

    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.gameObject.transform.name);
        if (crtPunchDelay != null)
        {
            if (FindComponent(collision.collider.transform, out Enemy enemy))
            {
                enemy.Kill();
            }
        }
    }
}