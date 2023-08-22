using TMPro;
using UnityEngine;

public class PowerManager : MonoBehaviourPlus
{
	//public string[] powerNames;
	IPlayerPower[] powers;
	int activePowerIndex = -1;
	TextMeshProUGUI powerNameText;

	void Start()
	{
		powers = new IPlayerPower[transform.childCount];
		for (int i = 0; i < powers.Length; i++) powers[i] = transform.GetChild(i).GetComponent<IPlayerPower>();
		powerNameText = transform.parent.Find("UI").Find("Power").GetComponent<TextMeshProUGUI>();
		for (int i = 0; i < powers.Length; i++)
		{
			if (powers[i].gameObject.activeInHierarchy)
			{
				SelectPower(i);//auto select the first power that is active in the hierarchy
				break;
			}
		}
		if (activePowerIndex == -1) SelectPower(0);//if no power was active already
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
		if (!InRange(activePowerIndex, 0, powers.Length) || TryDisable())//if no power is currently activated or the current power can be disabled
		{
			
			if (InRange(powerIndex, 0, powers.Length))// Activate the selected power
			{
				powers[powerIndex].gameObject.SetActive(true);
				activePowerIndex = powerIndex;
				powerNameText.text = powers[powerIndex].gameObject.name;
			}
			else
			{
				powerNameText.text = "";
			}
		}

		bool TryDisable()
		{
			if (powers[activePowerIndex].CanDisable)
			{
				powers[activePowerIndex].gameObject.SetActive(false);
				return true;
			}
			return false;
		}
	}

	public void RestrictPower(int powerIndex)
	{
		if (powerIndex >= 0 && powerIndex < powers.Length)
		{
			powers[powerIndex].gameObject.SetActive(false);
		}
	}

	public void EnablePower(int powerIndex)
	{
		if (powerIndex >= 0 && powerIndex < powers.Length)
		{
			powers[powerIndex].gameObject.SetActive(true);
		}
	}
}

public interface IPlayerPower
{
	public GameObject gameObject { get; }
	public bool CanDisable { get; }
}