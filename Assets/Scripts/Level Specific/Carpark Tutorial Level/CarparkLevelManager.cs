using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CarparkLevelManager : MonoBehaviour
{
    public Transform[] enemies;
    public ElevatorDoor exitElevator, enemyElevator;
    public TriggerCollider firstEnemyTrigger, elevatorTrigger, deleteInteriorTrigger, pillarTriggerCollider, pillarEnemiesTrigger;
    public UniversalButton elevatorButton;
    public WickUI wickUI;
    public float sceneLoadDelay;
    public Rotater[] boomSticks, afterWatchBoomSticks;
    public GameObject boomCollider, skyscraper, carparkInterior, afterWatchBoomCollider;
    public TimeScaler timeScaler;
    [HideInInspector] public bool timeScalerEnable;
    EnemyPathManager[] enemyPaths;

    IEnumerator Start()
    {
        //Init
        enemyPaths = new EnemyPathManager[enemies.Length];
        for (int i = 0; i < enemyPaths.Length; i++) enemyPaths[i] = new EnemyPathManager(enemies[i]);

        yield return null;//to work with debug checkpoints, not enough time to fix rn

        switch (CheckpointManager.instance.lastCheckpoint)
        {
            case 0:
                //Load area init
                skyscraper.SetActive(false);
                timeScaler.enabled = false;

                //Before elevator
                yield return new WaitUntil(() => firstEnemyTrigger.isTriggered);
                enemyPaths[0].SetNextEnemyPath();
                yield return new WaitUntil(() => enemyPaths[0].ActiveEnemiesAreDead());
                enemyElevator.Open();
                enemyPaths[0].SetNextEnemyPath(false);

                goto case 1;
            case 1:
                //Load area init
                //Destroy(enemies[0].gameObject);
                timeScaler.enabled = false;

                //Pick up watch
                yield return new WaitUntil(() => timeScalerEnable);
                timeScaler.enabled = true;
                float temp = wickUI.typeDelay;
                wickUI.typeDelay = .1f;
                wickUI.Display(new string[] { "You are", "John Matrix", "hold shift to slow time" }, () => wickUI.typeDelay = temp, false, 7); ;
                for (int i = 0; i < boomSticks.Length; i++) afterWatchBoomSticks[i].StartRotation();
                Destroy(afterWatchBoomCollider);

                //End of hallway
                yield return new WaitUntil(() => pillarEnemiesTrigger.isTriggered);
                for (int i = 0; i < 6; i++)
                {
                    enemyPaths[1].SetNextEnemyPath();
                    yield return new WaitForSeconds(.25f);
                }

                //Killed enemies
                yield return new WaitUntil(() => enemyPaths[1].ActiveEnemiesAreDead());
                wickUI.typeDelay = .1f;
                wickUI.UnDisplay(7);
                wickUI.typeDelay = temp;
                for (int i = 0; i < boomSticks.Length; i++) boomSticks[i].StartRotation();
                Destroy(boomCollider);
                skyscraper.SetActive(true);
                yield return new WaitUntil(() => deleteInteriorTrigger.isTriggered);
                if (Player.singlePlayer.weapon) Player.singlePlayer.input.Press("Drop");//drop weapon if holding one (because the animations don't look as good)

                goto case 2;
            case 2:
                //Load area init
                Destroy(carparkInterior);
                yield return new WaitUntil(() => pillarTriggerCollider.isTriggered);
                enemyPaths[2].SetNextEnemyPath(false);

                goto case 3;
            case 3:
                //Load area init
                if (carparkInterior != null) Destroy(carparkInterior);//if spawned at this checkpoint

                //Elevator section
                yield return new WaitUntil(() => exitElevator.IsFullyClosed && elevatorTrigger.isTriggered);//elevator is closed and player is inside elevator
                elevatorButton.interactable = false;
                yield return new WaitForSeconds(sceneLoadDelay);
                SceneManager.LoadSceneAsync("Mik Level", LoadSceneMode.Single);

                break;
        }
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
