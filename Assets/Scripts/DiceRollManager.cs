using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class DiceRollManager : MonoBehaviour
{
    [Header("Dice Settings")]
    public GameObject dicePrefab;
    public Transform spawnPoint;
    public int numberOfDice = 2;

    [Header("Physics Settings")]
    public float minForce = 5f;
    public float maxForce = 10f;
    public float minTorque = 5f;
    public float maxTorque = 15f;

    [Header("UI Components")]
    public Button rollButton;
    public TextMeshProUGUI rollResultText;

    [Header("Hero Movement")]
    public HeroPositionManager heroPositionManager;

    [Header("Floating Text")]
    public GameObject floatingTextPrefab;

    [Header("Dice Roll VFX & SFX")]
    public GameObject rollVFXPrefab;
    public Transform vfxSpawnPoint;
    public AudioClip rollSFX;
    public float rollSFXEarlyPlay = 0.1f; // 🔹 NEW: Adjust timing in Inspector

    [Header("Dice Disappear VFX & SFX")]
    public GameObject diceDisappearVFXPrefab;
    public AudioClip diceDisappearSFX;

    [Header("Dice Bounce SFX")]
    public AudioClip diceBounceSFX1;
    public AudioClip diceBounceSFX2;
    public float minBounceVolume = 0.2f;
    public float maxBounceVolume = 1.0f;

    [Header("Dice Spawn Delay")]
    public float spawnDiceDelay = 0.5f;

    private List<GameObject> spawnedDice = new List<GameObject>();
    private bool isRolling = false;
    private bool earnedFreeRoll = false;
    private AudioSource audioSource;

    void Start()
    {
        rollButton.onClick.AddListener(OnRollButtonClicked);
        UpdateRollButtonState();

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1.0f; // 3D Sound
    }

    void Update()
    {
        UpdateRollButtonState();
    }

    private void OnRollButtonClicked()
    {
        if (!isRolling)
        {
            isRolling = true;
            rollButton.interactable = false;
            StartCoroutine(PlayRollVFXandSFXWithDelay());
            StartCoroutine(RollDiceRoutine());
        }
    }

    private IEnumerator PlayRollVFXandSFXWithDelay()
    {
        if (rollSFXEarlyPlay > 0)
            yield return new WaitForSeconds(rollSFXEarlyPlay); // 🔹 NEW: Adjusted timing

        if (rollVFXPrefab != null && vfxSpawnPoint != null)
        {
            GameObject vfx = Instantiate(rollVFXPrefab, vfxSpawnPoint.position, Quaternion.identity);
            Destroy(vfx, 2f);
        }

        if (rollSFX != null && audioSource != null)
        {
            audioSource.PlayOneShot(rollSFX);
        }
    }

    private void PlayDiceDisappearVFXandSFX(Vector3 position)
    {
        if (diceDisappearVFXPrefab != null)
        {
            GameObject vfx = Instantiate(diceDisappearVFXPrefab, position, Quaternion.identity);
            Destroy(vfx, 2f);
        }

        if (diceDisappearSFX != null && audioSource != null)
        {
            audioSource.PlayOneShot(diceDisappearSFX);
        }
    }

    private void UpdateRollButtonState()
    {
        if (rollButton == null || StaminaManager.Instance == null || heroPositionManager == null)
            return;

        bool hasStamina = StaminaManager.Instance.currentStamina >= StaminaManager.Instance.staminaCostPerRoll;
        bool heroStopped = !heroPositionManager.IsMoving;

        rollButton.interactable = hasStamina && heroStopped && !isRolling;
    }

    IEnumerator RollDiceRoutine()
    {
        if (!StaminaManager.Instance.UseStamina(StaminaManager.Instance.staminaCostPerRoll))
        {
            rollResultText.text = "Not enough stamina!";
            isRolling = false;
            UpdateRollButtonState();
            yield break;
        }

        yield return StartCoroutine(ClearPreviousDiceWithEffects());

        yield return new WaitForSeconds(spawnDiceDelay);

        rollResultText.text = "Rolling...";
        List<int> diceResults = new List<int>();

        for (int i = 0; i < numberOfDice; i++)
        {
            Vector3 spawnPos = spawnPoint.position + Vector3.up * (2f + i * 0.5f);
            GameObject dice = Instantiate(dicePrefab, spawnPos, Random.rotation);
            Rigidbody rb = dice.GetComponent<Rigidbody>();

            DiceBounceSound diceSound = dice.AddComponent<DiceBounceSound>();
            diceSound.SetBounceSounds(diceBounceSFX1, diceBounceSFX2, minBounceVolume, maxBounceVolume);

            Vector3 forceDirection = new Vector3(
                Random.Range(-1f, 1f),
                1f,
                Random.Range(-1f, 1f)
            ).normalized;

            rb.AddForce(forceDirection * Random.Range(minForce, maxForce), ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * Random.Range(minTorque, maxTorque), ForceMode.Impulse);

            spawnedDice.Add(dice);
        }

        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(WaitForDiceToSettle());

        int total = 0;
        foreach (GameObject dice in spawnedDice)
        {
            DiceStats stats = dice.GetComponent<DiceStats>();
            diceResults.Add(stats.side);
            total += stats.side;
        }

        rollResultText.text = $"You Rolled: {total}";

        earnedFreeRoll = (diceResults.Count == 2 && diceResults[0] == diceResults[1]);

        heroPositionManager.MoveHero(total);

        yield return new WaitUntil(() => !heroPositionManager.IsMoving);

        if (earnedFreeRoll)
        {
            ShowFloatingText("FREE ROLL", Color.yellow);
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(RollDiceRoutine());
        }
        else
        {
            isRolling = false;
            UpdateRollButtonState();
        }
    }

    IEnumerator WaitForDiceToSettle()
    {
        bool diceStillMoving = true;
        float checkInterval = 0.2f;

        while (diceStillMoving)
        {
            yield return new WaitForSeconds(checkInterval);

            diceStillMoving = false;
            foreach (GameObject dice in spawnedDice)
            {
                Rigidbody rb = dice.GetComponent<Rigidbody>();
                if (rb.linearVelocity.magnitude > 0.05f || rb.angularVelocity.magnitude > 0.05f)
                {
                    diceStillMoving = true;
                    break;
                }
            }
        }
    }

    IEnumerator ClearPreviousDiceWithEffects()
    {
        foreach (var dice in spawnedDice)
        {
            PlayDiceDisappearVFXandSFX(dice.transform.position);
            Destroy(dice);
            yield return new WaitForSeconds(0.1f);
        }

        spawnedDice.Clear();
    }

    private void ShowFloatingText(string message, Color color)
    {
        if (floatingTextPrefab != null && heroPositionManager.hero != null)
        {
            GameObject textObj = Instantiate(floatingTextPrefab, heroPositionManager.hero.position + Vector3.up * 2, Quaternion.identity);
            FloatingText floatingText = textObj.GetComponent<FloatingText>();
            floatingText.Setup(message, color);
        }
    }
}
