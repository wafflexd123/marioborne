using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CarparkLevelManager : MonoBehaviour
{
	public GunTutorial gunTutorial;
	public EnemyPath[] enemySpawns;
	public ElevatorDoor elevator;
	public TriggerCollider elevatorTrigger;
	public ElevatorButton elevatorButton;
	public float sceneLoadDelay;

	IEnumerator Start()
	{
		yield return new WaitUntil(() => gunTutorial.weapon.BeingHeld());
		enemySpawns[0].SetPaths();
		gunTutorial.Destroy();
		yield return new WaitUntil(() => enemySpawns[0].AreDead());
		enemySpawns[1].SetPaths();
		elevator.Open();
		yield return new WaitUntil(() => enemySpawns[1].AreDead());
		yield return new WaitUntil(() => elevator.IsFullyClosed && elevatorTrigger.isTriggered);//elevator is closed and player is inside elevator
		elevatorButton.interactable = false;
		yield return new WaitForSeconds(sceneLoadDelay);
		SceneManager.LoadSceneAsync("After Carpark Level", LoadSceneMode.Single);
	}

	[System.Serializable]
	public class EnemyPath
	{
		public Enemy[] enemies;
		public Transform path;

		public bool AreDead()
		{
			for (int i = 0; i < enemies.Length; i++)
			{
				if (enemies[i] != null) return false;
			}
			return true;
		}

		public void SetPaths()
		{
			for (int i = 0; i < enemies.Length; i++)
			{
				enemies[i].points = path;
			}
		}
	}
}
