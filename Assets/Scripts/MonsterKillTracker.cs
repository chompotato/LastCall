using UnityEngine;
using TMPro;

public class MonsterKillTracker : MonoBehaviour
{
    public static MonsterKillTracker Instance; // Singleton instance

    [Header("UI Elements")]
    public TextMeshProUGUI killCountText; // UI Text for kills

    private int totalKills = 0; // Kill counter

    private void Awake()
    {
        // Ensure Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddKill()
    {
        totalKills++; // Increase kill count
        UpdateKillUI();
    }

    private void UpdateKillUI()
    {
        if (killCountText != null)
        {
            killCountText.text = $"Monsters Killed: {totalKills}"; // ✅ Update UI
        }
    }
}
