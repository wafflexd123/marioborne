using System.Collections;
using UnityEngine;

public class Fists : MonoBehaviourPlus
{
    public float punchDelay;
    Coroutine crtPunch;
    Player player;
    new Collider collider;
    private PlayerEnergy playerEnergy;

    private void Start()
    {
        collider = GetComponent<Collider>();
        player = GetComponentInParent<Player>();
        playerEnergy = Player.singlePlayer.GetComponent<PlayerEnergy>();
    }

    void Update()
    {
        if (Input.GetButtonDown("Attack")) Punch();
    }

    public void Punch()
    {
        if (player.weapon == null && crtPunch == null) crtPunch = StartCoroutine(Punch());//if not holding a weapon or punching
        IEnumerator Punch()
        {
            player.model.punch = true;
            collider.enabled = true;
            yield return new WaitUntil(() => !player.model.punch);
            collider.enabled = false;
            yield return new WaitForSeconds(punchDelay);
            crtPunch = null;
        }
    }

    void OnTriggerEnter(Collider collider)
    {
        if (FindComponent(collider.transform, out AIController enemy))
        {
            enemy.Kill(DeathType.Melee);
            playerEnergy.IncreaseEnergy(40);
        }
    }
}