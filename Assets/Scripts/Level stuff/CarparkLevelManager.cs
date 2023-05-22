using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CarparkLevelManager : MonoBehaviour
{
	public GunTutorial gunTutorial;
	public Transform enemies;
	public ElevatorDoor exitElevator, enemyElevator;
	public TriggerCollider elevatorTrigger, holdShiftTrigger;
	public ElevatorButton elevatorButton;
	public WickUI wickUI;
	public TriggerCollider firstEnemyTrigger, secondEnemyTrigger;
	public float sceneLoadDelay;
	public Rotater[] boomSticks;
	EnemyPathManager enemyPathManager;

	IEnumerator Start()
	{
		//Init
		enemyPathManager = new EnemyPathManager(enemies);
		StartCoroutine(Text());

		//Gun pickup section
		yield return new WaitUntil(() => firstEnemyTrigger.isTriggered || gunTutorial.weapon.BeingHeld());
		enemyPathManager.SetNextEnemyPath();
		enemyElevator.Open();
		enemyPathManager.SetNextEnemyPath();
		yield return new WaitUntil(() => enemyPathManager.ActiveEnemiesAreDead());
		for (int i = 0; i < boomSticks.Length; i++) boomSticks[i].StartRotation();

		yield return new WaitUntil(() => holdShiftTrigger.isTriggered);
		for (int i = 0; i < 6; i++) enemyPathManager.SetNextEnemyPath();

		//Elevator section
		exitElevator.Open();
		enemyPathManager.SetNextEnemyPath();
		yield return new WaitUntil(() => enemyPathManager.ActiveEnemiesAreDead());
		yield return new WaitUntil(() => exitElevator.IsFullyClosed && elevatorTrigger.isTriggered);//elevator is closed and player is inside elevator
		elevatorButton.interactable = false;
		yield return new WaitForSeconds(sceneLoadDelay);
		SceneManager.LoadSceneAsync("After Carpark Level", LoadSceneMode.Single);
	}

	IEnumerator Text()
	{
		wickUI.DisplayImmediate(new string[] { "You are" }, false);
		yield return new WaitUntil(() => gunTutorial.weapon.BeingHeld());
		gunTutorial.Destroy();
		wickUI.Display(new string[] { "You are", "John Matrix" }, null, false);
		yield return new WaitUntil(() => holdShiftTrigger.isTriggered);
		wickUI.DisplayImmediate(new string[] { "You are", "John Matrix", "hold shift to slow time" }, false);
		yield return new WaitUntil(() => Time.timeScale < 0.26f);
		wickUI.gameObject.SetActive(false);
	}
}
