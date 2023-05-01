using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeScaler : MonoBehaviourPlus
{
	public float timeScaleSpeed, minTimeScale, scaleDuration, recoveryDelay;
	float currentScaleDuration, recoveryTimer;
	Console.Line cnsTime;

	void Start()
	{
		cnsTime = Console.AddLine();
	}

	//this is kinda messy but it works lol
	//1.scale time down if holding button and duration isnt reached
	//2.scale time up if duration is reached or not holding button
	//3.reset duration only if time has been scaling up for resetdelay & button is not held
	void Update()
	{
		if (Input.GetButton("Ability") && currentScaleDuration < scaleDuration)//1.scale time down, increment scale duration, reset recovery timer
		{
			if (Time.timeScale > minTimeScale)
			{
				Time.timeScale -= timeScaleSpeed * Time.deltaTime;
				if (Time.timeScale < minTimeScale) Time.timeScale = minTimeScale;
			}
			currentScaleDuration += Time.unscaledDeltaTime;
			if (currentScaleDuration > scaleDuration) currentScaleDuration = scaleDuration;
			recoveryTimer = 0;
		}
		else if (!Input.GetButton("Ability") || currentScaleDuration >= scaleDuration)//2.scale time up, increment resetdelay timer
		{
			if (Time.timeScale < 1)
			{
				Time.timeScale += timeScaleSpeed * Time.deltaTime;
				if (Time.timeScale > 1) Time.timeScale = 1;
			}
			if (!Input.GetButton("Ability"))//3.decrement scale duration
			{
				recoveryTimer += Time.unscaledDeltaTime;
				if (recoveryTimer >= recoveryDelay)
				{
					recoveryTimer = recoveryDelay;
					currentScaleDuration -= Time.unscaledDeltaTime;
					if (currentScaleDuration < 0) currentScaleDuration = 0;
				}
			}
		}

		if (Console.Enabled) cnsTime.text = $"Hold shift to slow time\n" +
					$"Timescale: {Time.timeScale}, Unity timescale (should always be 1): {UnityEngine.Time.timeScale}\n" +
					$"Scale duration: {currentScaleDuration:#.00}, time to recover: {recoveryDelay - recoveryTimer:#.00}";
	}
}

