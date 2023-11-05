using System.Collections;
using UnityEngine;

public class Fists : MonoBehaviourPlus
{
	public float punchReloadTime;
	Coroutine crtPunch;
	new Collider collider;
	PlayerEnergy playerEnergy;

	private void Start()
	{
		collider = GetComponent<Collider>();
		playerEnergy = Player.singlePlayer.GetComponent<PlayerEnergy>();
	}

	void Update()
	{
		if (Input.GetButtonDown("Attack")) Punch();
	}

	public void Punch()
	{
		if (Player.singlePlayer.weapon == null && crtPunch == null) crtPunch = StartCoroutine(Punch());//if not holding a weapon or punching
		IEnumerator Punch()
		{
			Player.singlePlayer.handAnimator.Play("right_punch", 2);
			collider.enabled = true;
			yield return new WaitForSeconds(Player.singlePlayer.handAnimator.GetCurrentAnimatorStateInfo(2).length);
			collider.enabled = false;
			yield return new WaitForSeconds(punchReloadTime);
			crtPunch = null;
		}
	}

	void OnTriggerEnter(Collider collider)
	{
		if (FindComponent(collider.transform, out AIController enemy))
		{
			enemy.Kill(DeathType.Melee);
			playerEnergy.IncreaseEnergy(100);
		}
	}
}