using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class StaminaManager : MonoBehaviour
{
    [Header("Stamina Settings")]
    public int maxStamina = 100;
    public int currentStamina;
    public int staminaCostPerRoll = 10;

    [Header("Recharge Settings")]
    public float rechargeAmount = 5f;
    public float rechargeInterval = 1f;

    [Header("UI Elements")]
    public TextMeshProUGUI staminaText;
    public TextMeshProUGUI maxStaminaText;
    public TextMeshProUGUI rechargeRateText;
    public Slider staminaBar;
    public Transform hero;
    public Vector3 staminaBarOffset = new Vector3(0, 2f, 0);

    [Header("Floating Text Settings")]
    public GameObject floatingTextPrefab;

    private float rechargeTimer;
    private static List<GameObject> activeFloatingTexts = new List<GameObject>();

    public static StaminaManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        currentStamina = maxStamina;
        UpdateStaminaUI();
    }

    private void Update()
    {
        HandleStaminaRecharge();
        UpdateStaminaBarPosition();
    }

    private void HandleStaminaRecharge()
    {
        if (currentStamina < maxStamina)
        {
            rechargeTimer += Time.deltaTime;
            if (rechargeTimer >= rechargeInterval)
            {
                rechargeTimer -= rechargeInterval;
                int rechargeAmountRounded = Mathf.RoundToInt(rechargeAmount);
                currentStamina = Mathf.Min(currentStamina + rechargeAmountRounded, maxStamina);
                ShowFloatingText($"+{rechargeAmountRounded} Stamina", Color.cyan);
                UpdateStaminaUI();
            }
        }
        else
        {
            rechargeTimer = 0f;
        }
    }

    public bool UseStamina(int amount)
    {
        if (currentStamina >= amount)
        {
            currentStamina -= amount;
            ShowFloatingText($"-{amount} Stamina", Color.red);
            UpdateStaminaUI();
            return true;
        }
        return false;
    }

    public void RechargeStamina(int amount)
    {
        currentStamina = Mathf.Min(currentStamina + amount, maxStamina);
        ShowFloatingText($"+{amount} Stamina", Color.cyan);
        UpdateStaminaUI();
    }

    public void ModifyMaxStamina(int amount)
    {
        maxStamina = Mathf.Max(10, maxStamina + amount);
        currentStamina = Mathf.Min(currentStamina, maxStamina);
        ShowFloatingText($"+{amount} Max Stamina", new Color(1f, 0.5f, 0f)); // Orange text
        UpdateStaminaUI();
    }

    public void ModifyRechargeRate(float amount)
    {
        rechargeAmount = Mathf.Max(0, rechargeAmount + amount);
        ShowFloatingText($"+{amount:F1} Recharge Rate", Color.green);
        UpdateStaminaUI();
    }

    private void UpdateStaminaUI()
    {
        if (staminaText != null)
            staminaText.text = $"Stamina: {currentStamina}/{maxStamina}";

        if (maxStaminaText != null)
            maxStaminaText.text = $"Max Stamina: {maxStamina}";

        if (rechargeRateText != null)
            rechargeRateText.text = $"Recharge Rate: {rechargeAmount:F1}/sec";

        if (staminaBar != null)
        {
            staminaBar.maxValue = maxStamina;
            staminaBar.value = currentStamina;
        }
    }

    private void UpdateStaminaBarPosition()
    {
        if (hero != null && staminaBar != null)
        {
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(hero.position + staminaBarOffset);
            staminaBar.transform.position = screenPosition;
        }
    }

    private void ShowFloatingText(string message, Color color)
    {
        if (floatingTextPrefab == null || hero == null) return;

        Vector3 basePosition = hero.position + Vector3.up * 2;
        Vector3 offset = new Vector3(Random.Range(-0.5f, 0.5f), activeFloatingTexts.Count * 0.3f, 0);
        Vector3 spawnPosition = basePosition + offset;

        GameObject textObj = Instantiate(floatingTextPrefab, spawnPosition, Quaternion.identity);
        FloatingText floatingText = textObj.GetComponent<FloatingText>();
        floatingText?.Setup(message, color);

        activeFloatingTexts.Add(textObj);
        StartCoroutine(RemoveFloatingTextAfterDelay(textObj, floatingText.fadeDuration));
    }

    private IEnumerator RemoveFloatingTextAfterDelay(GameObject textObj, float delay)
    {
        yield return new WaitForSeconds(delay);
        activeFloatingTexts.Remove(textObj);
    }
}
