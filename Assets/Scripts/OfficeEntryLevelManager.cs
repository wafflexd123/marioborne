using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OfficeEntryLevelManager : MonoBehaviour
{
	public ElevatorDoor entryElevator, exitElevator;
	public WeaponBase startWeapon;
	public Transform enemies;

	IEnumerator Start()
	{
		yield return new WaitUntil(() => startWeapon.BeingHeld());//wait for player to pickup weapon
		yield return new WaitForSeconds(1f);
		entryElevator.Open();
		yield return new WaitUntil(() => enemies.childCount == 0);//wait for all enemies to be killed
		exitElevator.Open();
	}
}
