using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CarparkLevelManager : MonoBehaviour
{
	public GunTutorial gunTutorial;
	public Transform enemies;
	public ElevatorDoor elevator;
	public TriggerCollider elevatorTrigger;
	public ElevatorButton elevatorButton;
	public WickUI wickUI;
	public TriggerCollider firstEnemyTrigger;
	public float sceneLoadDelay;
	EnemyPathManager enemyPathManager;

	IEnumerator Start()
	{
		//Init
		enemyPathManager = new EnemyPathManager(enemies);
		StartCoroutine(Text());

		//Gun pickup section
		yield return new WaitUntil(() => firstEnemyTrigger.isTriggered);
		float delay = .05f;
		for (int i = 0; i < 8; i++)
		{
			enemyPathManager.SetNextEnemyPath();
			yield return new WaitForSeconds(delay);
		}
		yield return new WaitUntil(() => enemyPathManager.ActiveEnemiesAreDead());

		//Elevator section
		elevator.Open();
		enemyPathManager.SetNextEnemyPath();
		yield return new WaitUntil(() => enemyPathManager.ActiveEnemiesAreDead());
		yield return new WaitUntil(() => elevator.IsFullyClosed && elevatorTrigger.isTriggered);//elevator is closed and player is inside elevator
		elevatorButton.interactable = false;
		yield return new WaitForSeconds(sceneLoadDelay);
		SceneManager.LoadSceneAsync("After Carpark Level", LoadSceneMode.Single);
	}

	IEnumerator Text()
	{
		yield return new WaitUntil(() => gunTutorial.weapon.BeingHeld());
		gunTutorial.Destroy();
		//float originalDelay = wickUI.typeDelay;
		//wickUI.typeDelay = 0.03f;
		wickUI.Display(new string[] { "You are", "John Matrix", "hold shift to slow time" }, /*() => wickUI.typeDelay = originalDelay*/null, false);
		yield return new WaitUntil(() => Time.timeScale < 0.26f);
		wickUI.gameObject.SetActive(false);
	}
}
