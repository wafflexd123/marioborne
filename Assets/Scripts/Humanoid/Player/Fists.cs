using System.Collections;
using UnityEngine;

public class Fists : MonoBehaviourPlus
{
	public float punchTime, punchReloadTime;
	public Vector3 punchVector;
	public GameObject punchEffect;
	public AudioPool.Clips punchClips, punchHitClips;
	Coroutine crtPunch;
	new Collider collider;
	PlayerEnergy playerEnergy;
	new AudioPool audio;

	private void Start()
	{
		collider = GetComponent<Collider>();
		playerEnergy = Player.singlePlayer.GetComponent<PlayerEnergy>();
		punchEffect = Instantiate(punchEffect);
		audio = gameObject.AddComponent<AudioPool>().Initialise(punchTime + punchReloadTime, punchClips.MaxShotLength() + punchHitClips.MaxShotLength());
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
			punchEffect.SetActive(false);
			punchEffect.transform.position = Player.singlePlayer.camera.transform.position + Player.singlePlayer.camera.transform.TransformDirection(punchVector);
			punchEffect.SetActive(true);
			collider.enabled = true;
			punchClips.PlayRandom(audio);
			yield return new WaitForSeconds(punchTime);
			collider.enabled = false;
			yield return new WaitForSeconds(punchReloadTime);
			Player.singlePlayer.handAnimator.Play("empty", 2);
			crtPunch = null;
		}
	}

	void OnTriggerEnter(Collider collider)
	{
		if (FindComponent(collider.transform, out AIController enemy))
		{
			punchHitClips.PlayRandom(audio);
			enemy.Kill(DeathType.Melee);
			playerEnergy.IncreaseEnergy(100);

		}
	}

	private void OnDisable()//called when a weapon is picked up
	{
		StopCoroutine(ref crtPunch);
		collider.enabled = false;
	}
}