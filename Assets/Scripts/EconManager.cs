using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class EconManager : MonoBehaviour
{
    [Header("Gold Settings")]
    public int startingGold = 100;
    public int CurrentGold { get; private set; }

    [Header("UI Elements")]
    public TextMeshProUGUI goldText;

    [Header("Floating Text Settings")]
    public GameObject floatingTextPrefab;
    public Transform hero;

    [Header("Coin Gain VFX & SFX")]
    public GameObject coinVFXHero; // VFX when hero gains coins
    public AudioClip coinGainSFX1;
    public AudioClip coinGainSFX2;
    public AudioClip coinGainSFX3;
    public float coinGainVolume = 1.0f;

    private AudioSource audioSource;
    private static List<GameObject> activeFloatingTexts = new List<GameObject>();

    public static EconManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        CurrentGold = startingGold;
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f; // 2D sound

        UpdateGoldUI(); // 🔹 FIX: Ensure the gold balance appears on game start
    }

    public void AddGold(int amount)
    {
        CurrentGold += amount;
        ShowFloatingText($"+{amount} Gold", Color.yellow);
        PlayRandomCoinGainSFX();
        PlayCoinGainVFX();

        UpdateGoldUI();
    }

    public bool SpendGold(int amount)
    {
        if (CurrentGold >= amount)
        {
            CurrentGold -= amount;
            ShowFloatingText($"-{amount} Gold", Color.red);
            UpdateGoldUI();
            return true;
        }
        return false;
    }

    private void PlayRandomCoinGainSFX()
    {
        if (audioSource != null)
        {
            AudioClip selectedClip = GetRandomCoinSFX();
            if (selectedClip != null)
            {
                audioSource.PlayOneShot(selectedClip, coinGainVolume);
            }
        }
    }

    private void PlayCoinGainVFX()
    {
        if (coinVFXHero != null && hero != null)
        {
            GameObject vfx = Instantiate(coinVFXHero, hero.position + Vector3.up * 2, Quaternion.identity);
            Destroy(vfx, 1.5f);
        }
    }

    private AudioClip GetRandomCoinSFX()
    {
        List<AudioClip> availableSFX = new List<AudioClip>();

        if (coinGainSFX1 != null) availableSFX.Add(coinGainSFX1);
        if (coinGainSFX2 != null) availableSFX.Add(coinGainSFX2);
        if (coinGainSFX3 != null) availableSFX.Add(coinGainSFX3);

        if (availableSFX.Count == 0) return null;

        return availableSFX[Random.Range(0, availableSFX.Count)];
    }

    private void UpdateGoldUI()
    {
        if (goldText != null)
            goldText.text = $"Gold: {CurrentGold}";
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
