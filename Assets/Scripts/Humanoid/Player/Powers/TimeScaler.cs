using UnityEngine;
using UnityEngine.UI;

public class TimeScaler : MonoBehaviourPlus
{
	public string inputButton;
	public float timeScaleSpeed, scaleDuration, recoveryDelay;
	public Image imgTimeleft;
	new AudioPool audio;
	public AudioPool.Clip audioTimeSlow;
	float currentScaleDuration, recoveryTimer;
	private bool audioPlayed;
	//Console.Line cnsTime;

	void Awake()
	{
		//cnsTime = Console.AddLine();
		imgTimeleft.gameObject.SetActive(false);
		audio = gameObject.AddComponent<AudioPool>().Initialise(1);
		//startUIWidth = -ui.rect.width;
	}

	//void Update()
	//{
	//	if (Input.GetButton(inputButton))
	//	{
	//		if (Time.timeScale > Time.minTimeScale)
	//		{
	//			Time.timeScale -= Time.deltaTime * timeScaleSpeed;
	//			if (Time.timeScale < Time.minTimeScale) Time.timeScale = Time.minTimeScale;
	//		}
	//	}
	//	else if (Time.timeScale < 1)
	//	{
	//		Time.timeScale += Time.deltaTime * timeScaleSpeed;
	//		if (Time.timeScale > 1) Time.timeScale = 1;
	//	}
	//}

	//this is kinda messy but it works lol
	//1.scale time down if holding button and duration isnt reached
	//2.scale time up if duration is reached or not holding button
	//3.reset duration only if time has been scaling up for resetdelay & button is not held
	void Update()
	{
		if (Input.GetButton(inputButton) && currentScaleDuration < scaleDuration)//1.scale time down, increment scale duration, reset recovery timer
		{
            if (audioPlayed)
            {
				audioTimeSlow.Play(audio);
				audioPlayed = false;
            }
			if (Time.timeScale > Time.minTimeScale)
			{
				Time.timeScale -= timeScaleSpeed * Time.deltaTime;
				if (Time.timeScale < Time.minTimeScale) Time.timeScale = Time.minTimeScale;
				//imgTimescale.fillAmount = Mathf.InverseLerp(minTimeScale, 1, Time.timeScale);
			}
			currentScaleDuration += Time.unscaledDeltaTime;
			if (currentScaleDuration > scaleDuration) currentScaleDuration = scaleDuration;
			recoveryTimer = 0;
			imgTimeleft.gameObject.SetActive(true);
			imgTimeleft.fillAmount = 1 - currentScaleDuration / scaleDuration;
		}
		else if (!Input.GetButton(inputButton) || currentScaleDuration >= scaleDuration)//2.scale time up, increment resetdelay timer
		{
			audioPlayed = true;
			if (Time.timeScale < 1)
			{
				Time.timeScale += timeScaleSpeed * Time.deltaTime;
				if (Time.timeScale > 1) Time.timeScale = 1;
				//imgTimescale.fillAmount = Mathf.InverseLerp(minTimeScale, 1, Time.timeScale);

			}
			if (!Input.GetButton(inputButton))//3.decrement scale duration
			{
				recoveryTimer += Time.unscaledDeltaTime;
				if (recoveryTimer >= recoveryDelay)
				{
					recoveryTimer = recoveryDelay;
					currentScaleDuration -= Time.unscaledDeltaTime;
					if (currentScaleDuration < 0)
					{
						currentScaleDuration = 0;
						imgTimeleft.gameObject.SetActive(false);
					}
					else
					{
						imgTimeleft.gameObject.SetActive(true);
						imgTimeleft.fillAmount = 1 - currentScaleDuration / scaleDuration;
					}
				}
			}
		}

		//if (Console.Enabled) cnsTime.text =
		//			$"Timescale: {Time.timeScale}, Unity timescale (should always be 1): {UnityEngine.Time.timeScale}\n" +
		//			$"Scale duration: {currentScaleDuration:#.00}, time to recover: {recoveryDelay - recoveryTimer:#.00}";
	}
}