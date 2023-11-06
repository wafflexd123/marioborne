using System.Collections;
using UnityEngine;

public class Fists : MonoBehaviourPlus
{
	public float punchTime, punchReloadTime;
	public Vector3 punchVector;
	public Punch punchPrefab;
	public AudioPool.Clips punchClips, punchHitClips;
	new AudioPool audio;
	Coroutine crtPunch;

	private void Start()
	{
		audio = gameObject.AddComponent<AudioPool>().Initialise(punchTime + punchReloadTime, punchClips.MaxShotLength());
	}

	void Update()
	{
		if (Input.GetButtonDown("Attack")) Punch();
	}

	public void Punch()
	{
		if (crtPunch == null) crtPunch = StartCoroutine(Punch());//if not holding a weapon or punching
		IEnumerator Punch()
		{
			Player.singlePlayer.handAnimator.Play("right_punch", 2);
			punchPrefab.Spawn(punchTime, punchHitClips, Player.singlePlayer.camera.transform.position + Player.singlePlayer.camera.transform.TransformDirection(punchVector));
			punchClips.PlayRandom(audio);
			yield return new WaitForSeconds(punchReloadTime);
			Player.singlePlayer.handAnimator.Play("empty", 2);
			crtPunch = null;
		}
	}
}