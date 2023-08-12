using System.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UI;

public class Fists : MonoBehaviourPlus
{
	public float punchDelay, deflectDelay, deflectTime;
	Coroutine crtDeflect, crtPunch;
	[SerializeField] ReflectWindow reflectWindow;
	Player player;
	Image deflectPercent;
	new Collider collider;
	bool _isFiring, isDeflecting;

	public bool IsFiring => _isFiring;

	private void Start()
	{
		collider = GetComponent<Collider>();
		player = GetComponentInParent<Player>();
		reflectWindow = Instantiate(reflectWindow).Initialise(player);
		deflectPercent = player.transform.Find("UI").Find("Deflect").GetComponent<Image>();
		player.input.AddListener("Mouse", InputType.OnPress, (float direction) =>
		{
			if (!FindComponent(player.raycast.transform, out WeaponBase weapon) || Vector3.Distance(player.transform.position, weapon.transform.position) >= player.maxInteractDistance)//if player is not looking at a weapon or weapon is too far away
			{
				if (direction < 0) LeftMouse();
				else RightMouse();
			}
		});
	}

	void LeftMouse()
	{
		if (player.weapon == null && crtPunch == null && !isDeflecting)
			crtPunch = StartCoroutine(Punch());//if not holding a weapon, deflecting or punching

		IEnumerator Punch()
		{
			player.model.punch = true;
			collider.enabled = true;
			_isFiring = true;
			yield return new WaitUntil(() => !player.model.punch);
			_isFiring = false;
			collider.enabled = false;
			yield return new WaitForSeconds(punchDelay);
			crtPunch = null;
		}
	}

	void RightMouse()
	{
		if (crtDeflect == null && crtPunch == null && (player.weapon == null || !player.weapon.IsFiring))
			crtDeflect = StartCoroutine(Deflect());//if not firing a weapon, punching or waiting for deflect delay

		IEnumerator Deflect()
		{
			player.model.deflect = true;
			reflectWindow.enabled = true;
			_isFiring = true;
			isDeflecting = true;
			for (float timer = 0; timer < deflectTime; timer += Time.fixedDeltaTime) yield return new WaitForFixedUpdate();
			isDeflecting = false;
			_isFiring = false;
			reflectWindow.enabled = false;

			deflectPercent.gameObject.SetActive(true);
			for (float timer = 0; timer < deflectDelay; timer += Time.fixedDeltaTime)
			{
				deflectPercent.fillAmount = timer / deflectDelay;
				yield return new WaitForFixedUpdate();
			}
			deflectPercent.gameObject.SetActive(false);

			crtDeflect = null;
		}
	}

	void OnTriggerEnter(Collider collider)
	{
		if (FindComponent(collider.transform, out Enemy enemy))
		{
			enemy.Kill(DeathType.Melee);
		}
	}
}