using System.Collections;
using TMPro;
using UnityEngine;

public class AmmoCounter : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI currentAmmoText;
	[SerializeField] private TextMeshProUGUI nextAmmoText;

	public void SetAmmo(int newAmmo)
	{
		nextAmmoText.text = newAmmo > 0 ? $"{newAmmo}" : "Q";
		StartCoroutine(ChangeAmmoEffect());
	}

	private IEnumerator ChangeAmmoEffect()
	{
		float moveAmount = 0.0468f;

		Vector3 currentStartPos = new Vector3(0, 0, 0);
		Vector3 nextStartPos = new Vector3(0, moveAmount, 0);

		currentAmmoText.rectTransform.localPosition = currentStartPos;
		nextAmmoText.rectTransform.localPosition = nextStartPos;

		Vector3 currentEndPos = currentStartPos - moveAmount * Vector3.up;
		Vector3 nextEndPos = nextStartPos - moveAmount * Vector3.up;

		float duration = 0.1f;
		float elapsed = 0f;

		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			float t = elapsed / duration;

			currentAmmoText.rectTransform.localPosition = Vector3.Lerp(currentStartPos, currentEndPos, t);
			nextAmmoText.rectTransform.localPosition = Vector3.Lerp(nextStartPos, nextEndPos, t);

			yield return null;
		}

		currentAmmoText.text = nextAmmoText.text;
		currentAmmoText.rectTransform.localPosition = currentStartPos;
		nextAmmoText.rectTransform.localPosition = nextStartPos;
	}
}
