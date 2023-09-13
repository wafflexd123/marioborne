using TMPro;
using UnityEngine;

public class PowerManager : MonoBehaviourPlus
{
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
		int scroll = (int)Input.GetAxisRaw("Mouse ScrollWheel");
		if (scroll != 0)
		{
			SelectPower((activePowerIndex + scroll) % powers.Length);
		}
		else
		{
			for (int i = 0; i < powers.Length; i++)
			{
				if (Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha1 + i)))//smart
				{
					SelectPower(i);
				}
			}
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

	public void SelectPower<T>(bool forceDisable = false) where T : IPlayerPower
	{
		for (int i = 0; i < powers.Length; i++)
		{
			if (powers[i].GetType() == typeof(T))
			{
				if (TryDisableCurrentPower(forceDisable)) EnableNewPower(i);
				return;
			}
		}
	}

	public void SelectPower(int powerIndex, bool forceDisable = false)
	{
		if (TryDisableCurrentPower(forceDisable))//Deactivate the currently active power
		{
			if (InRange(powerIndex, 0, powers.Length)) EnableNewPower(powerIndex);
			else powerNameText.text = "";
		}
	}

	private void EnableNewPower(int powerIndex)
	{
		powers[powerIndex].gameObject.SetActive(true);
		activePowerIndex = powerIndex;
		powerNameText.text = powers[powerIndex].gameObject.name;
	}

	private bool TryDisableCurrentPower(bool forceDisable)//if no power is currently activated or the current power can be disabled
	{
		if (!InRange(activePowerIndex, 0, powers.Length)) return true;//if no power is currently selected
		if (powers[activePowerIndex].CanDisable || forceDisable)//if can disable
		{
			powers[activePowerIndex].gameObject.SetActive(false);
			return true;
		}
		return false;
	}
}

public interface IPlayerPower
{
	public GameObject gameObject { get; }
	public bool CanDisable { get; }
}