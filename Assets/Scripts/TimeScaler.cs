using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeScaler : MonoBehaviourPlus
{
	public float timeScaleSpeed, minTimeScale;
	Console.Line cnsTime;

	void Start()
	{
		cnsTime = Console.AddLine();
	}

	void Update()
	{
		if (Input.GetButton("Ability"))
		{
			if (Time.timeScale > minTimeScale)
			{
				float timeScale = Time.timeScale - (timeScaleSpeed * Time.deltaTime);
				if (timeScale < minTimeScale) timeScale = minTimeScale;
				Time.timeScale = timeScale;
			}
		}
		else if (Time.timeScale < 1)
		{
			float timeScale = Time.timeScale + (timeScaleSpeed * Time.deltaTime);
			if (timeScale > 1) timeScale = 1;
			Time.timeScale = timeScale;
		}

		if (Console.Enabled) cnsTime.text = $"Hold ability button [E] to slow time\nTimescale: {Time.timeScale}, Unity timescale (should always be 1): {UnityEngine.Time.timeScale}";
	}
}
