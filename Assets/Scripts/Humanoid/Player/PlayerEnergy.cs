using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Manages the player's energy, allowing for increasing, decreasing, and setting energy values.
/// Handles passive energy regeneration and provides a uniform rate of change for the energy UI update.
/// </summary>
public class PlayerEnergy : MonoBehaviour
{
    [Header("Energy Properties")]
    [Tooltip("The maximum energy the player can have.")]
    [SerializeField] private int maxEnergy = 100;

    [Tooltip("Amount of energy passively regenerated over the regen interval.")]
    [SerializeField] private int passiveRegenRate = 1;

    [Tooltip("Time in seconds for how often energy should regenerate.")]
    [SerializeField] private float regenInterval = 1f;

    [Header("UI Properties")]
    [Tooltip("Reference to the TextMesh Pro UI component displaying energy.")]
    [SerializeField] private TextMeshProUGUI energyText;

    [Tooltip("Time it takes for the displayed energy to lerp to the current energy value.")]
    [SerializeField] private float lerpDuration = 1f;

    private int currentEnergy;
    private float displayedEnergy;
    private float lerpStartTime;
    private float initialDisplayedEnergy;

    private void Start()
    {
        currentEnergy = maxEnergy;
        displayedEnergy = maxEnergy;
        StartCoroutine(PassiveRegen());
    }

    /// <summary>
    /// Increases the player's energy by the specified amount and starts the energy UI update.
    /// </summary>
    /// <param name="amount">Amount to increase energy by.</param>
    public void IncreaseEnergy(int amount)
    {
        currentEnergy = Mathf.Min(currentEnergy + amount, maxEnergy);
        StartEnergyLerp();
    }

    /// <summary>
    /// Decreases the player's energy by the specified amount and starts the energy UI update.
    /// </summary>
    /// <param name="amount">Amount to decrease energy by.</param>
    public void DecreaseEnergy(int amount)
    {
        currentEnergy = Mathf.Max(currentEnergy - amount, 0);
        StartEnergyLerp();
    }

    /// <summary>
    /// Sets the player's energy to the specified value.
    /// </summary>
    /// <param name="amount">Value to set the energy to, clamped between 0 and maxEnergy.</param>
    public void SetEnergy(int amount)
    {
        currentEnergy = Mathf.Clamp(amount, 0, maxEnergy);
        StartEnergyLerp();
    }

    public int GetEnergy()
    {
        return currentEnergy;
    }

    /// <summary>
    /// Periodically regenerates energy over time.
    /// </summary>
    private IEnumerator PassiveRegen()
    {
        while (true)
        {
            yield return new WaitUntil(() => displayedEnergy == currentEnergy);
            IncreaseEnergy(passiveRegenRate);
            yield return new WaitForSeconds(regenInterval);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            DecreaseEnergy(25);
        }
    }

    /// <summary>
    /// Begins the process to smoothly update the displayed energy value.
    /// </summary>
    private void StartEnergyLerp()
    {
        StopCoroutine("EnergyLerp");
        lerpStartTime = UnityEngine.Time.time;
        initialDisplayedEnergy = displayedEnergy;
        StartCoroutine("EnergyLerp");
    }

    /// <summary>
    /// Coroutine that provides a uniform rate of change for updating the displayed energy value.
    /// </summary>
    private IEnumerator EnergyLerp()
    {
        while (Mathf.Abs(displayedEnergy - currentEnergy) > 0.01f)
        {
            float elapsed = UnityEngine.Time.time - lerpStartTime;
            float percentage = elapsed / lerpDuration;

            displayedEnergy = Mathf.Lerp(initialDisplayedEnergy, currentEnergy, percentage);
            energyText.text = Mathf.RoundToInt(displayedEnergy).ToString();

            yield return null;
        }

        displayedEnergy = currentEnergy;  // Ensure displayedEnergy is set to currentEnergy at the end
        energyText.text = Mathf.RoundToInt(displayedEnergy).ToString();
    }
}
