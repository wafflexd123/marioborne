using TMPro;
using UnityEngine;

public class PowerManager : MonoBehaviour
{
	public GameObject[] powers;
	//public string[] powerNames;
	int activePowerIndex = -1;
	TextMeshProUGUI powerNameText;

	void Start()
	{
		powerNameText = transform.Find("UI").Find("Power").GetComponent<TextMeshProUGUI>();
		for (int i = 0; i < powers.Length; i++)
		{
			if (powers[i].activeInHierarchy)
			{
				SelectPower(i);//auto select the first power that is active in the hierarchy
				break;
			}
		}
		if (activePowerIndex == -1) SelectPower(0);
	}

	void Update()
	{
		for (int i = 0; i < powers.Length; i++)
		{
			if (Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha1 + i)))//smart
			{
				SelectPower(i);
			}
		}
	}

	public void SelectPower(int powerIndex)
	{
		// Deactivate the currently active power
		if (activePowerIndex >= 0 && activePowerIndex < powers.Length)
		{
			powers[activePowerIndex].SetActive(false);
		}
		// Activate the selected power
		if (powerIndex >= 0 && powerIndex < powers.Length)
		{
			powers[powerIndex].SetActive(true);
			activePowerIndex = powerIndex;
			powerNameText.text = powers[powerIndex].name;
			//if (powerIndex < powerNames.Length)
			//{
			//    powerNameText.text = powerNames[powerIndex];
			//}
		}
	}
	public void RestrictPower(int powerIndex)
	{
		if (powerIndex >= 0 && powerIndex < powers.Length)
		{
			powers[powerIndex].SetActive(false);
		}
	}
	public void EnablePower(int powerIndex)
	{
		if (powerIndex >= 0 && powerIndex < powers.Length)
		{
			powers[powerIndex].SetActive(true);
		}
	}
}
