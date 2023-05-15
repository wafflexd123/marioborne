using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VignetteControl : MonoBehaviour
{
	public float fadeOutSpeed = 2.0f;
	public float flashDuration = 0.5f;

	private Image redVignetteImage;
	private float timer;
	private bool shouldFlash;

	void Start()
	{
		redVignetteImage = GetComponent<Image>();
		timer = 0;
		shouldFlash = false;
		SetVignetteAlpha(0f);
	}

	void Updatex()
	{
		if (shouldFlash)
		{
			timer += Time.deltaTime;
			if (timer >= flashDuration)
			{
				shouldFlash = false;
				timer = 0;
			}
		}
		else
		{
			FadeOutVignette();
		}
	}

	// Call this method to trigger the effect. You can set the amount of time it stays on the script in the player.
	public void TriggerVignetteFlash()
	{
		Debug.Log("Red Vignette Flash");
		shouldFlash = true;
		timer = 0;
		SetVignetteAlpha(1f);
	}
	// Don't worry about this one.
	private void FadeOutVignette()
	{
		Color currentColor = redVignetteImage.color;
		float targetAlpha = 0f;
		currentColor.a = Mathf.Lerp(currentColor.a, targetAlpha, fadeOutSpeed * Time.deltaTime);
		redVignetteImage.color = currentColor;
	}
	// You can call this to either enable/disable the effect permanently. This is for keeping it on after you dieded.
	public void SetVignetteAlpha(float alpha)
	{
		Color currentColor = redVignetteImage.color;
		currentColor.a = alpha;
		redVignetteImage.color = currentColor;
	}

	public void SetVignetteAlpha(float currentHealth, float maxHealth)
	{
		redVignetteImage.color = new Color(redVignetteImage.color.r, redVignetteImage.color.g, redVignetteImage.color.b, 1 - Mathf.InverseLerp(0, maxHealth, currentHealth));
	}
}
