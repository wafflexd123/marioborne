using System.Collections;
using UnityEngine;

public class Punch : MonoBehaviourPlus
{
	public float deleteTime;
	float punchTime;
	new AudioPool audio;
	new Collider collider;
	AudioPool.Clips punchHitClips;

	public void Spawn(float punchTime, AudioPool.Clips punchHitClips, Vector3 position)
	{
		Punch punch = Instantiate(gameObject, position, Quaternion.identity).GetComponent<Punch>();
		punch.collider = punch.GetComponent<Collider>();
		punch.punchHitClips = punchHitClips;
		punch.punchTime = punchTime;
		punch.audio = punch.gameObject.AddComponent<AudioPool>().Initialise(punchTime, punchHitClips.MaxShotLength());
		punch.StartCoroutine(punch.PunchRoutine());
	}

	IEnumerator PunchRoutine()
	{
		yield return new WaitForSeconds(punchTime);
		collider.enabled = false;
		yield return new WaitForSeconds(deleteTime);
		Destroy(gameObject);
	}

	void OnTriggerEnter(Collider collider)
	{
		if (FindComponent(collider.transform, out AIController enemy))
		{
			punchHitClips.PlayRandom(audio);
			enemy.Kill(DeathType.Melee);
			Player.singlePlayer.GetComponent<PlayerEnergy>().SetEnergy(100);
		}
	}
}
