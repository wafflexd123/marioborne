using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CarparkLevelManager : MonoBehaviour
{
	public GunTutorial gunTutorial;
	public Transform enemies;
	public ElevatorDoor exitElevator, enemyElevator;
	public TriggerCollider firstEnemyTrigger, elevatorTrigger, holdShiftTrigger, deleteInteriorTrigger;
	public UniversalButton elevatorButton;
	public WickUI wickUI;
	public float sceneLoadDelay;
	public Rotater[] boomSticks;
	public GameObject boomCollider, skyscraper, carparkInterior;
	EnemyPathManager enemyPathManager;

	IEnumerator Start()
	{
		//Init
		enemyPathManager = new EnemyPathManager(enemies);
		skyscraper.SetActive(false);

		//Before elevator
		yield return new WaitUntil(() => firstEnemyTrigger.isTriggered);
		enemyPathManager.SetNextEnemyPath();
		yield return new WaitUntil(() => enemyPathManager.ActiveEnemiesAreDead());
		enemyElevator.Open();
		enemyPathManager.SetNextEnemyPath(false);

		//Through first hallway
		yield return new WaitUntil(() => holdShiftTrigger.isTriggered);
		for (int i = 0; i < 6; i++)
		{
			enemyPathManager.SetNextEnemyPath();
			yield return new WaitForSeconds(.25f);
		}
		float temp = wickUI.typeDelay;
		wickUI.typeDelay = .1f;
		wickUI.Display(new string[] { "You are", "John Matrix", "hold shift to slow time" }, () => wickUI.typeDelay = temp, false, 7);
		yield return new WaitUntil(() => Time.timeScale < 0.26f || enemyPathManager.ActiveEnemiesAreDead());
		wickUI.UnDisplay(7);

		//After big battle
		for (int i = 0; i < boomSticks.Length; i++) boomSticks[i].StartRotation();
		Destroy(boomCollider);
		skyscraper.SetActive(true);
		yield return new WaitUntil(() => deleteInteriorTrigger.isTriggered);
		Destroy(carparkInterior);

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
