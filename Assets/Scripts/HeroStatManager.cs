using UnityEngine;
using TMPro;

public class HeroStatManager : MonoBehaviour
{
    [Header("Hero Attributes")]
    public int strength = 10;
    public int intelligence = 10;
    public int willpower = 10;
    public int dexterity = 10;
    public float movementSpeed = 3f; // New Movement Speed Stat

    [Header("UI Elements")]
    public TextMeshProUGUI strengthText;
    public TextMeshProUGUI intelligenceText;
    public TextMeshProUGUI willpowerText;
    public TextMeshProUGUI dexterityText;
    public TextMeshProUGUI movementSpeedText;

    private HeroPositionManager heroPositionManager;

    private void Start()
    {
        heroPositionManager = GetComponent<HeroPositionManager>();

        if (heroPositionManager != null)
        {
            heroPositionManager.walkSpeed = movementSpeed;
            heroPositionManager.runSpeed = movementSpeed * 1.5f; // Run is 1.5x walk speed
        }

        UpdateStatsUI();
    }

    public void ModifyStrength(int amount)
    {
        strength = Mathf.Max(0, strength + amount);
        UpdateStatsUI();
    }

    public void ModifyIntelligence(int amount)
    {
        intelligence = Mathf.Max(0, intelligence + amount);
        UpdateStatsUI();
    }

    public void ModifyWillpower(int amount)
    {
        willpower = Mathf.Max(0, willpower + amount);
        UpdateStatsUI();
    }

    public void ModifyDexterity(int amount)
    {
        dexterity = Mathf.Max(0, dexterity + amount);
        UpdateStatsUI();
    }

    public void ModifyMovementSpeed(float amount)
    {
        movementSpeed = Mathf.Max(1f, movementSpeed + amount); // Ensures speed doesn't go below 1
        if (heroPositionManager != null)
        {
            heroPositionManager.walkSpeed = movementSpeed;
            heroPositionManager.runSpeed = movementSpeed * 1.5f;
        }
        UpdateStatsUI();
    }

    private void UpdateStatsUI()
    {
        if (strengthText != null) strengthText.text = $"Strength: {strength}";
        if (intelligenceText != null) intelligenceText.text = $"Intelligence: {intelligence}";
        if (willpowerText != null) willpowerText.text = $"Willpower: {willpower}";
        if (dexterityText != null) dexterityText.text = $"Dexterity: {dexterity}";
        if (movementSpeedText != null) movementSpeedText.text = $"Speed: {movementSpeed:F1}";
    }
}
