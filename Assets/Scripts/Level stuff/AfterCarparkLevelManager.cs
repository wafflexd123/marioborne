using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AfterCarparkLevelManager : MonoBehaviour
{
	public TriggerCollider firstEnemyTrigger;
	public EnemyPath[] enemySpawns;
	public ElevatorDoor elevator;
	public TriggerCollider elevatorTrigger;
	public UniversalButton elevatorButton;
	public float sceneLoadDelay;

	IEnumerator Start()
	{
		Debug.Log("Stage 0");
		yield return new WaitUntil(() => firstEnemyTrigger.isTriggered);
		Debug.Log("Stage 1");
		enemySpawns[0].SetPaths();
		yield return new WaitUntil(() => elevator.IsFullyClosed && elevatorTrigger.isTriggered);//elevator is closed and player is inside elevator
		Debug.Log("Stage 2");
		elevatorButton.interactable = false;
		yield return new WaitForSeconds(sceneLoadDelay);
		Debug.Log("Stage 3");
		SceneManager.LoadSceneAsync("Level Select", LoadSceneMode.Single);
	}
}
