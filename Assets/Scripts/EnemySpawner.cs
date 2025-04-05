using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    public GameObject enemyPrefab; // Enemy prefab to spawn
    public Transform spawnPoint;   // Optional spawn location

    [Header("Enemy Behavior Options")]
    public bool follow = false;  // If true, enemies will follow the hero
    public bool patrol = false;  // If true, enemies will patrol

    [Header("Patrol Settings")]
    public float patrolRadius = 5f; // Max distance from spawn point if no patrol target
    public float patrolWaitTime = 2f; // Time before moving to next patrol point
    public GameObject patrolTarget; // Optional patrol target

    [Header("Floating Stamina Bar")]
    public GameObject staminaBarPrefab; // Assign the UI Stamina Bar prefab

    [Header("UI")]
    public Button spawnButton; // UI Button to trigger spawning

    private void Start()
    {
        if (spawnButton != null)
        {
            spawnButton.onClick.AddListener(SpawnEnemy);
        }
        else
        {
            Debug.LogWarning("EnemySpawner: No button assigned in the Inspector.");
        }
    }

    public void SpawnEnemy()
    {
        if (enemyPrefab != null)
        {
            Vector3 spawnPosition = spawnPoint ? spawnPoint.position : transform.position;
            GameObject newEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            Enemy enemyScript = newEnemy.GetComponent<Enemy>();

            if (enemyScript != null)
            {
                SetupStaminaBar(newEnemy, enemyScript);

                if (follow)
                {
                    StartCoroutine(FollowHero(enemyScript));
                }
                else if (patrol)
                {
                    StartCoroutine(PatrolRoutine(enemyScript, spawnPosition));
                }
                LogEnemyStats(enemyScript);
            }
        }
        else
        {
            Debug.LogError("EnemySpawner: No enemy prefab assigned!");
        }
    }

    private void SetupStaminaBar(GameObject enemy, Enemy enemyScript)
    {
        if (staminaBarPrefab != null)
        {
            GameObject staminaBarInstance = Instantiate(staminaBarPrefab, enemy.transform);
            staminaBarInstance.transform.localPosition = new Vector3(0, 2f, 0); // Adjust height above enemy

            Slider staminaSlider = staminaBarInstance.GetComponentInChildren<Slider>();
            if (staminaSlider != null)
            {
                enemyScript.SetStaminaBar(staminaSlider); // Connect slider to enemy
            }
        }
        else
        {
            Debug.LogWarning("EnemySpawner: No stamina bar prefab assigned!");
        }
    }

    private IEnumerator FollowHero(Enemy enemy)
    {
        Transform hero = FindFirstObjectByType<HeroPositionManager>()?.transform;

        while (hero != null && enemy != null) // ✅ Stops when enemy is destroyed
        {
            if (enemy == null) yield break; // ✅ Exit if enemy is destroyed

            Vector3 direction = (hero.position - enemy.transform.position).normalized;
            enemy.transform.position += direction * enemy.movementSpeed * Time.deltaTime;

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, targetRotation, 5f * Time.deltaTime);
            }
            yield return null;
        }
    }

    private IEnumerator PatrolRoutine(Enemy enemy, Vector3 spawnPos)
    {
        while (patrol && enemy != null) // ✅ Stops when enemy is destroyed
        {
            Vector3 targetPosition;

            if (patrolTarget != null)
            {
                targetPosition = Vector3.Distance(enemy.transform.position, spawnPos) < 1f ? patrolTarget.transform.position : spawnPos;
            }
            else
            {
                targetPosition = spawnPos + new Vector3(
                    Random.Range(-patrolRadius, patrolRadius),
                    0,
                    Random.Range(-patrolRadius, patrolRadius)
                );
            }

            yield return StartCoroutine(MoveToPosition(enemy, targetPosition));

            yield return new WaitForSeconds(patrolWaitTime);
        }
    }

    private IEnumerator MoveToPosition(Enemy enemy, Vector3 target)
    {
        while (enemy != null && Vector3.Distance(enemy.transform.position, target) > 0.2f)
        {
            if (enemy == null) yield break; // ✅ Exit if enemy is destroyed

            Vector3 direction = (target - enemy.transform.position).normalized;
            enemy.transform.position += direction * (enemy.movementSpeed / 2) * Time.deltaTime;

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, targetRotation, 5f * Time.deltaTime);
            }

            yield return null;
        }
    }

    private void LogEnemyStats(Enemy enemy)
    {
        Debug.Log($"Spawned Enemy Stats:\n" +
                  $"Movement Speed: {enemy.movementSpeed}\n" +
                  $"Follow: {follow}\n" +
                  $"Patrol: {patrol}");
    }
}
