using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Attributes")]
    public int maxStamina = 100;
    public int currentStamina;
    public float staminaRechargeRate = 5f;
    public float staminaRechargeDelay = 2f;

    [Header("Movement")]
    public float movementSpeed = 2f;

    [Header("Death Effects")]
    public AudioClip deathSFX; // ✅ Sound when enemy dies
    public GameObject deathVFXPrefab; // ✅ Visual effect when enemy dies

    private Slider staminaBar;
    private float rechargeTimer = 0f;
    private Transform cameraTransform;
    private AudioSource audioSource;

    private void Start()
    {
        currentStamina = maxStamina;
        cameraTransform = Camera.main.transform;
        audioSource = gameObject.AddComponent<AudioSource>(); // ✅ Ensure we have an AudioSource
        UpdateStaminaBar();
    }

    private void Update()
    {
        HandleStaminaRecharge();
        RotateStaminaBar();
    }

    public void SetStaminaBar(Slider slider)
    {
        staminaBar = slider;
        staminaBar.maxValue = maxStamina;
        staminaBar.value = currentStamina;
    }

    public bool UseStamina(int amount)
    {
        if (currentStamina > 0)
        {
            currentStamina -= amount;
            rechargeTimer = 0f;
            UpdateStaminaBar();

            if (currentStamina <= 0)
            {
                Die();
            }
            return true;
        }
        return false;
    }

    private void HandleStaminaRecharge()
    {
        if (currentStamina < maxStamina)
        {
            rechargeTimer += Time.deltaTime;
            if (rechargeTimer >= staminaRechargeDelay)
            {
                currentStamina = Mathf.Min(maxStamina, currentStamina + Mathf.RoundToInt(staminaRechargeRate * Time.deltaTime));
                UpdateStaminaBar();
            }
        }
    }

    private void UpdateStaminaBar()
    {
        if (staminaBar != null)
        {
            staminaBar.value = currentStamina;
        }
    }

    private void RotateStaminaBar()
    {
        if (staminaBar != null && cameraTransform != null)
        {
            staminaBar.transform.LookAt(cameraTransform);
            staminaBar.transform.Rotate(0, 180f, 0);
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} has been defeated!");

        // ✅ Increase kill count
        if (MonsterKillTracker.Instance != null)
        {
            MonsterKillTracker.Instance.AddKill();
        }

        // ✅ Play death VFX
        if (deathVFXPrefab != null)
        {
            GameObject vfx = Instantiate(deathVFXPrefab, transform.position, Quaternion.identity);
            Destroy(vfx, 2f); // Remove VFX after 2 seconds
        }

        // ✅ Play death SFX
        if (deathSFX != null)
        {
            audioSource.PlayOneShot(deathSFX);
        }

        // ✅ Remove stamina bar
        if (staminaBar != null)
        {
            Destroy(staminaBar.gameObject);
        }

        // ✅ Destroy enemy after sound plays (if sound exists)
        float destroyDelay = (deathSFX != null) ? deathSFX.length : 0f;
        Destroy(gameObject, destroyDelay);
    }
}
