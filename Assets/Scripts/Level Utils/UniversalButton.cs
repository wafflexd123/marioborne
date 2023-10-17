using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class UniversalButton : Raycastable
{
    public GameObject ui;
    public bool interactable = true;
    public UnityEvent action;
    Coroutine crtRaycast;

	private void Start()
	{
        if (ui != null) ui.SetActive(false);
	}

	public override void OnRaycast(Player player)
    {
        if (interactable)
        {
            if (Input.GetButtonDown("Interact"))
            {
                action.Invoke();
            }
            if (ui != null && crtRaycast == null) crtRaycast = StartCoroutine(WaitForEndRaycast(player));
        }
    }

    IEnumerator WaitForEndRaycast(Player player)
    {
        ui.SetActive(true);
        while (FindComponent(player.raycast.transform, out UniversalButton _))//while looking at this
        {
            yield return null;
        }
        ui.SetActive(false);
        crtRaycast = null;
    }
}
