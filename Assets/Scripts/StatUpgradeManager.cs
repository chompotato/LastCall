using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatUpgradeManager : MonoBehaviour
{
    [Header("Upgrade Settings")]
    public int upgradeCost = 20;
    public int costIncrement = 10;

    [Header("Stat Upgrade Amounts")]
    public float strengthIncrease = 1f;
    public float intelligenceIncrease = 1f;
    public float willpowerIncrease = 1f;
    public float dexterityIncrease = 1f;
    public float movementSpeedIncrease = 0.5f;
    public float staminaMaxIncrease = 5f;
    public float staminaRechargeIncrease = 0.5f;

    [Header("UI Elements")]
    public Button upgradeButton;
    public Button spendAllButton;
    public TextMeshProUGUI upgradeCostText;

    [Header("Floating Text Settings")]
    public GameObject floatingTextPrefab;
    public Transform hero; // Hero reference for floating text positioning

    private HeroStatManager heroStatManager;
    private StaminaManager staminaManager;
    private EconManager econManager;

    private void Start()
    {
        heroStatManager = FindFirstObjectByType<HeroStatManager>();
        staminaManager = FindFirstObjectByType<StaminaManager>();
        econManager = EconManager.Instance;

        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(UpgradeRandomStat);

        if (spendAllButton != null)
            spendAllButton.onClick.AddListener(SpendAllGoldOnUpgrades);

        UpdateUI();
    }

    private void UpgradeRandomStat()
    {
        if (!econManager.SpendGold(upgradeCost)) return;

        UpgradeStat();
        upgradeCost += costIncrement;
        UpdateUI();
    }

    private void SpendAllGoldOnUpgrades()
    {
        while (econManager.CurrentGold >= upgradeCost)
        {
            if (!econManager.SpendGold(upgradeCost)) break;
            UpgradeStat();
            upgradeCost += costIncrement;
        }
        UpdateUI();
    }

    private void UpgradeStat()
    {
        int statToUpgrade = Random.Range(0, 7);
        float amount = 0f;
        string statName = "";
        Color textColor = Color.yellow;

        switch (statToUpgrade)
        {
            case 0:
                heroStatManager.ModifyStrength((int)strengthIncrease);
                amount = strengthIncrease;
                strengthIncrease += 0.1f;
                statName = "Strength";
                break;
            case 1:
                heroStatManager.ModifyIntelligence((int)intelligenceIncrease);
                amount = intelligenceIncrease;
                intelligenceIncrease += 0.1f;
                statName = "Intelligence";
                break;
            case 2:
                heroStatManager.ModifyWillpower((int)willpowerIncrease);
                amount = willpowerIncrease;
                willpowerIncrease += 0.1f;
                statName = "Willpower";
                break;
            case 3:
                heroStatManager.ModifyDexterity((int)dexterityIncrease);
                amount = dexterityIncrease;
                dexterityIncrease += 0.1f;
                statName = "Dexterity";
                break;
            case 4:
                heroStatManager.ModifyMovementSpeed(movementSpeedIncrease);
                amount = movementSpeedIncrease;
                movementSpeedIncrease += 0.01f;
                statName = "Speed";
                break;
            case 5:
                staminaManager.ModifyMaxStamina((int)staminaMaxIncrease);
                amount = staminaMaxIncrease;
                staminaMaxIncrease += 1f;
                statName = "Max Stamina";
                textColor = Color.cyan;
                break;
            case 6:
                staminaManager.ModifyRechargeRate(staminaRechargeIncrease);
                amount = staminaRechargeIncrease;
                staminaRechargeIncrease += 0.05f;
                statName = "Stamina Recharge";
                textColor = Color.cyan;
                break;
        }

        ShowFloatingText($"+{amount} {statName}", textColor);
    }

    private void UpdateUI()
    {
        upgradeCostText.text = $"Upgrade Cost: {upgradeCost}";
        upgradeButton.interactable = econManager.CurrentGold >= upgradeCost;
        spendAllButton.interactable = econManager.CurrentGold >= upgradeCost;
    }

    private void ShowFloatingText(string message, Color color)
    {
        if (floatingTextPrefab != null && hero != null)
        {
            GameObject textObj = Instantiate(floatingTextPrefab, hero.position + Vector3.up * 2, Quaternion.identity);
            FloatingText floatingText = textObj.GetComponent<FloatingText>();
            floatingText.Setup(message, color);
        }
    }
}
