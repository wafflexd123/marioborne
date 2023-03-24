using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeScaler : WeaponBase
{
	public float timeScaleSpeed, minTimeScale;
	Console.Line cnsTime;

	private void Start()
	{
		cnsTime = Console.AddLine();
	}

	protected override void Update()
	{
		base.Update();

		if (Input.GetMouseButton(mouseButton))
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

		cnsTime.text = $"Timescale: {Time.timeScale}, Unity timescale (should always be 1): {UnityEngine.Time.timeScale}";
	}
}
