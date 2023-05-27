using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CarparkLevelManager : MonoBehaviour
{
	public GunTutorial gunTutorial;
	public Transform[] enemies;
	public ElevatorDoor exitElevator, enemyElevator;
	public TriggerCollider firstEnemyTrigger, elevatorTrigger, holdShiftTrigger, deleteInteriorTrigger;
	public UniversalButton elevatorButton;
	public WickUI wickUI;
	public float sceneLoadDelay;
	public Rotater[] boomSticks;
	public GameObject boomCollider, skyscraper, carparkInterior;
	EnemyPathManager[] enemyPaths;

	IEnumerator Start()
	{
		//Init
		enemyPaths = new EnemyPathManager[enemies.Length];
		for (int i = 0; i < enemyPaths.Length; i++) enemyPaths[i] = new EnemyPathManager(enemies[i]);

		switch (CheckpointManager.instance.lastCheckpoint)
		{
			case 0:
				//Load area init
				skyscraper.SetActive(false);

				//Before elevator
				yield return new WaitUntil(() => firstEnemyTrigger.isTriggered);
				enemyPaths[0].SetNextEnemyPath();
				yield return new WaitUntil(() => enemyPaths[0].ActiveEnemiesAreDead());
				enemyElevator.Open();
				enemyPaths[0].SetNextEnemyPath(false);

				goto case 1;
			case 1:
				//Load area init
				Destroy(enemies[0].gameObject);

				//Through first hallway
				yield return new WaitUntil(() => holdShiftTrigger.isTriggered || Player.singlePlayer.hand.childCount > 0);
				for (int i = 0; i < 6; i++)
				{
					enemyPaths[1].SetNextEnemyPath();
					yield return new WaitForSeconds(.25f);
				}
				float temp = wickUI.typeDelay;
				wickUI.typeDelay = .1f;
				wickUI.Display(new string[] { "You are", "John Matrix", "hold shift to slow time" }, null, false, 7);

				//Killed enemies
				yield return new WaitUntil(() => enemyPaths[1].ActiveEnemiesAreDead());
				wickUI.UnDisplay(7);
				wickUI.typeDelay = temp;
				for (int i = 0; i < boomSticks.Length; i++) boomSticks[i].StartRotation();
				Destroy(boomCollider);
				skyscraper.SetActive(true);
				yield return new WaitUntil(() => deleteInteriorTrigger.isTriggered);

				goto case 2;
			case 2:
				//Load area init
				Destroy(carparkInterior);

				goto case 3;
			case 3:
				//Load area init
				if (carparkInterior != null) Destroy(carparkInterior);//if spawned at this checkpoint

				break;
		}
		//Elevator section
		//exitElevator.Open();
		//enemyPathManager.SetNextEnemyPath();
		//yield return new WaitUntil(() => enemyPathManager.ActiveEnemiesAreDead());
		//yield return new WaitUntil(() => exitElevator.IsFullyClosed && elevatorTrigger.isTriggered);//elevator is closed and player is inside elevator
		//elevatorButton.interactable = false;
		//yield return new WaitForSeconds(sceneLoadDelay);
		//SceneManager.LoadSceneAsync("After Carpark Level", LoadSceneMode.Single);
	}

	//IEnumerator Text()
	//{
	//	wickUI.DisplayImmediate(new string[] { "You are" }, false);
	//	yield return new WaitUntil(() => gunTutorial.weapon.BeingHeld());
	//	gunTutorial.Destroy();
	//	wickUI.Display(new string[] { "You are", "John Matrix" }, null, false);
	//	yield return new WaitUntil(() => holdShiftTrigger.isTriggered);
	//	wickUI.DisplayImmediate(new string[] { "You are", "John Matrix", "hold shift to slow time" }, false);
	//	yield return new WaitUntil(() => Time.timeScale < 0.26f);
	//	wickUI.gameObject.SetActive(false);
	//}
}
