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

    [Header("Flash Properties")]
    [Tooltip("The color to flash when energy is insufficient.")]
    [SerializeField] private Color flashColor = Color.red;
    [Tooltip("Duration of the flash effect.")]
    [SerializeField] private float flashDuration = 0.5f;

    private Material energyTextMaterial;
    private Color originalEmissionColor;
    private int currentEnergy;
    private float displayedEnergy;
    private float lerpStartTime;
    private float initialDisplayedEnergy;

    private void Start()
    {
        currentEnergy = maxEnergy;
        displayedEnergy = maxEnergy;
        energyTextMaterial = energyText.fontMaterial;
        originalEmissionColor = energyTextMaterial.GetColor("_GlowColor");
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
    /// Flashes the energy text color when the player doesn't have enough energy.
    /// </summary>
    public void FlashEnergyText()
    {
        StartCoroutine(FlashEnergyTextCoroutine());
    }

    private IEnumerator FlashEnergyTextCoroutine()
    {
        // Save the original vertex color
        Color originalVertexColor = energyText.color;
        // Save the original glow color and intensity
        Color originalGlowColor = energyTextMaterial.GetColor("_GlowColor");
        float originalGlowIntensity = energyTextMaterial.GetFloat("_GlowPower");

        // Set the vertex color to red (or any flash color you want)
        energyText.color = flashColor;

        // Set the glow color with increased intensity
        Color highIntensityGlowColor = flashColor * 2.0f; // Increase intensity by multiplying the color
        energyTextMaterial.SetColor("_GlowColor", highIntensityGlowColor);
        energyTextMaterial.SetFloat("_GlowPower", 2.0f); // Adjust this value as needed for your desired intensity

        // Update the material properties
        energyTextMaterial.EnableKeyword("_EMISSION");
        energyText.UpdateMeshPadding(); // Update padding in case glow affects text bounds

        // Wait for the duration of the flash
        yield return new WaitForSeconds(flashDuration);

        // Restore the original vertex color
        energyText.color = originalVertexColor;
        // Restore the original glow color and intensity
        energyTextMaterial.SetColor("_GlowColor", originalGlowColor);
        energyTextMaterial.SetFloat("_GlowPower", originalGlowIntensity);

        // Update the material properties
        energyTextMaterial.DisableKeyword("_EMISSION");
        energyText.UpdateMeshPadding(); // Update padding to original
    }

    /// <summary>
    /// Decreases the player's energy by the specified amount and starts the energy UI update.
    /// If the player does not have enough energy, flash the energy text.
    /// </summary>
    /// <param name="amount">Amount to decrease energy by.</param>
    public void DecreaseEnergy(int amount)
    {
        if (currentEnergy >= amount)
        {
            currentEnergy = Mathf.Max(currentEnergy - amount, 0);
            StartEnergyLerp();
        }
        else
        {
            // Player tried to use more energy than they have, flash the energy text
            FlashEnergyText();
        }
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
