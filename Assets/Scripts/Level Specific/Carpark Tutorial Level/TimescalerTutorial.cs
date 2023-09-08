using System.Collections;
using UnityEngine;

public class TimescalerTutorial : Raycastable
{
    public GameObject ui, delete;
    Coroutine crtRaycast;
    public CarparkLevelManager carparkLevelManager;

    public override void OnRaycast(Player player)
    {
        if (ui != null && crtRaycast == null) crtRaycast = StartCoroutine(WaitForEndRaycast());
        IEnumerator WaitForEndRaycast()
        {
            ui.SetActive(true);
            while (FindComponent(player.raycast.transform, out TimescalerTutorial scaler))//while looking at this
            {
                ui.transform.LookAt(player.camera.transform);
                if (Input.GetMouseButtonDown(0))//if clicked on watch
                {
                    carparkLevelManager.timeScalerEnable = true;
                    Destroy(delete);
                    transform.parent = player.transform;
                    StartCoroutine(MoveToPosLocal(new Position(player.transform, true), 5f, transform, () => Destroy(gameObject), .1f));
                    yield break;
                }
                yield return null;
            }
            ui.SetActive(false);
            crtRaycast = null;
        }
    }
}
