using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.PlayerLoop;

/// <summary>
/// Manages the player's energy, allowing for increasing, decreasing, and setting energy values.
/// Also handles passive energy regeneration and updates the UI accordingly.
/// </summary>
public class PlayerEnergy : MonoBehaviour
{
    [Header("Energy Properties")]
    [SerializeField] private int maxEnergy = 100; // The maximum energy the player can have
    [SerializeField] private int passiveRegenRate = 1; // The amount of energy passively regenerated over regenInterval
    [SerializeField] private float regenInterval = 1f; // Time in seconds for how often energy should regenerate

    [Header("UI Properties")]
    [SerializeField] private TextMeshProUGUI energyText; // Reference to the TextMesh Pro UI component displaying energy

    private int currentEnergy; // The player's current energy level
    private float displayedEnergy; // Used to smoothly interpolate the energy value on the UI

    private void Start()
    {
        currentEnergy = maxEnergy;
        displayedEnergy = maxEnergy;
        StartCoroutine(PassiveRegen());
        UpdateEnergyUI();
    }

    /// <summary>
    /// Increases the player's energy by a specified amount.
    /// </summary>
    /// <param name="amount">Amount of energy to increase.</param>
    public void IncreaseEnergy(int amount)
    {
        currentEnergy = Mathf.Min(currentEnergy + amount, maxEnergy);
    }

    /// <summary>
    /// Decreases the player's energy by a specified amount.
    /// </summary>
    /// <param name="amount">Amount of energy to decrease.</param>
    public void DecreaseEnergy(int amount)
    {
        currentEnergy = Mathf.Max(currentEnergy - amount, 0);
    }

    /// <summary>
    /// Directly sets the player's energy to a specified amount.
    /// </summary>
    /// <param name="amount">Target energy value. Clamped between 0 and maxEnergy.</param>
    public void SetEnergy(int amount)
    {
        currentEnergy = Mathf.Clamp(amount, 0, maxEnergy);
    }

    /// <summary>
    /// Coroutine that handles the passive regeneration of energy.
    /// </summary>
    /// <returns>Yields WaitForSeconds based on the regenInterval.</returns>
    private IEnumerator PassiveRegen()
    {
        while (true)
        {
            IncreaseEnergy(passiveRegenRate);
            yield return new WaitForSeconds(regenInterval);
        }
    }

    private void Update()
    {
        UpdateEnergyUI();

        if (Input.GetKeyDown(KeyCode.B))
        {
            DecreaseEnergy(25);
        }
    }

    /// <summary>
    /// Updates the displayed energy on the UI by interpolating towards the current energy value.
    /// </summary>
    private void UpdateEnergyUI()
    {
        displayedEnergy = Mathf.Lerp(displayedEnergy, currentEnergy, 0.05f);
        energyText.text = Mathf.RoundToInt(displayedEnergy).ToString();
    }
}
