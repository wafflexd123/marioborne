using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Fists : MonoBehaviourPlus
{
	public float punchTime, punchDelay, deflectTime, deflectDelay;
	Coroutine crtDeflect, crtPunch;
	GameObject reflectWindow;
	Player player;
	Image deflectPercent;
	new Collider collider;
	bool _isFiring, isDeflecting;

	public bool IsFiring => _isFiring;

	private void Start()
	{
		collider = GetComponent<Collider>();
		player = GetComponentInParent<Player>();
		reflectWindow = player.transform.Find("ReflectWindow").gameObject;
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
			player.model.punching = true;
			collider.enabled = true;
			_isFiring = true; 
			yield return new WaitForSeconds(punchTime);
			_isFiring = false;
			player.model.punching = false;
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
			reflectWindow.SetActive(true);
			_isFiring = true;
			isDeflecting = true;
			yield return new WaitForSeconds(deflectTime);
			isDeflecting = false;
			_isFiring = false;
			reflectWindow.SetActive(false);
			player.model.deflect = false;

			float timer = 0;
			deflectPercent.gameObject.SetActive(true);
			while (timer < deflectDelay)
			{
				timer += Time.fixedDeltaTime;
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

	//protected void Update()
	//{
	//	if (!player.hasDied)
	//	{
	//		if (player.hand.childCount > 0)
	//		{
	//			Enable(false);
	//		}
	//		else if (FindComponent(player.raycast.transform, out WeaponBase weapon))
	//		{
	//			if (Vector3.Distance(player.transform.position, weapon.transform.position) >= player.maxInteractDistance)
	//			{
	//				if (Input.GetMouseButtonDown(0)) LeftMouse();
	//				if (Input.GetMouseButtonDown(1)) RightMouse();
	//			}
	//		}
	//		else
	//		{
	//			if (Input.GetMouseButtonDown(0)) LeftMouse();
	//			if (Input.GetMouseButtonDown(1)) RightMouse();
	//		}
	//	}
	//}

	//player.model.deflect = true;
	//				crtDeflectTime = StartCoroutine(Anim());

	//				IEnumerator Anim()
	//				{
	//					reflectWindow.SetActive(true);
	//					yield return new WaitForSeconds(deflectTime);
	//					reflectWindow.SetActive(false);
	//					player.model.deflect = false;
	//					crtDeflectDelay = StartCoroutine(Delay());
	//					crtDeflectTime = null;
	//				}
	//			}
	//		}

	//		IEnumerator Delay()
	//		{
	//			//yield return new WaitForSeconds(deflectDelay);
	//			float timer = 0;
	//			deflectPercent.gameObject.SetActive(true);
	//			while (timer < deflectDelay)
	//			{
	//				timer += Time.fixedDeltaTime;
	//				deflectPercent.fillAmount = timer / deflectDelay;
	//				yield return new WaitForFixedUpdate();
	//			}
	//			deflectPercent.gameObject.SetActive(false);
	//			crtDeflectDelay = null;
	//		}

	//protected void LeftMouse()
	//{
	//	if (player.hand.childCount <= 0)
	//	{
	//		if (crtDeflectTime == null)
	//		{
	//			if (crtPunchDelay == null && player.LookingAt != Vector3.negativeInfinity)//if not waiting for fireDelay && wielder is looking at something
	//			{
	//				if (crtPunchTime == null)
	//				{
	//					player.model.punching = true;
	//					Enable(true);
	//					crtPunchTime = StartCoroutine(Anim());

	//					IEnumerator Anim()
	//					{
	//						yield return new WaitForSeconds(punchTime);
	//						Enable(false);
	//						player.model.punching = false;
	//						crtPunchDelay = StartCoroutine(Delay());
	//						crtPunchTime = null;
	//					}
	//				}
	//			}

	//			IEnumerator Delay()
	//			{
	//				yield return new WaitForSeconds(punchDelay);
	//				crtPunchDelay = null;
	//			}
	//		}
	//	}
	//}

	//protected void RightMouse() //handles deflection
	//{
	//	if (player.hand.childCount <= 0)
	//	{
	//		if (crtPunchTime == null)
	//		{
	//			if (crtDeflectDelay == null && player.LookingAt != Vector3.negativeInfinity)
	//			{
	//				if (crtDeflectTime == null)
	//				{
	//					player.model.deflect = true;
	//					crtDeflectTime = StartCoroutine(Anim());

	//					IEnumerator Anim()
	//					{
	//						reflectWindow.SetActive(true);
	//						yield return new WaitForSeconds(deflectTime);
	//						reflectWindow.SetActive(false);
	//						player.model.deflect = false;
	//						crtDeflectDelay = StartCoroutine(Delay());
	//						crtDeflectTime = null;
	//					}
	//				}
	//			}

	//			IEnumerator Delay()
	//			{
	//				//yield return new WaitForSeconds(deflectDelay);
	//				float timer = 0;
	//				deflectPercent.gameObject.SetActive(true);
	//				while (timer < deflectDelay)
	//				{
	//					timer += Time.fixedDeltaTime;
	//					deflectPercent.fillAmount = timer / deflectDelay;
	//					yield return new WaitForFixedUpdate();
	//				}
	//				deflectPercent.gameObject.SetActive(false);
	//				crtDeflectDelay = null;
	//			}
	//		}
	//	}
	//}

	//private void Enable(bool enable)
	//{
	//	if (enable) GetComponent<BoxCollider>().enabled = true;
	//	else GetComponent<BoxCollider>().enabled = false;
	//}
}