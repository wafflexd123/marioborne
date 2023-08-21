using TMPro;
using UnityEngine;

public class PowerManager : MonoBehaviour
{
    public MonoBehaviour[] powers;
    public string[] powerNames;
    public int activePowerIndex = 0;
    public TextMeshProUGUI powerNameText;

    void Start()
    {
        SelectPower(0);
    }

    void Update()
    {
        for (int i = 0; i < powers.Length; i++)
        {
            if (Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha1 + i)))
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
            powers[activePowerIndex].enabled = false;
        }
        // Activate the selected power
        if (powerIndex >= 0 && powerIndex < powers.Length)
        {
            powers[powerIndex].enabled = true;
            activePowerIndex = powerIndex;

            if (powerIndex < powerNames.Length)
            {
                powerNameText.text = powerNames[powerIndex];
            }
        }
    }
    public void RestrictPower(int powerIndex)
    {
        if (powerIndex >= 0 && powerIndex < powers.Length)
        {
            powers[powerIndex].enabled = false;
        }
    }
    public void EnablePower(int powerIndex)
    {
        if (powerIndex >= 0 && powerIndex < powers.Length)
        {
            powers[powerIndex].enabled = true;
        }
    }
}
