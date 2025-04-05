using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HeroPositionManager : MonoBehaviour
{
    [Header("Hero and Path Settings")]
    public Transform hero;
    public List<Transform> pathNodes = new List<Transform>();

    [Header("Movement Settings")]
    public float walkSpeed = 2f;
    public float runSpeed = 5f;
    public float runDistanceThreshold = 3f;
    public float acceleration = 3f;
    public float deceleration = 3f;
    public float rotationSpeed = 5f;
    public float nodeStopThreshold = 0.05f;

    [Header("UI Settings")]
    public TextMeshProUGUI movesLeftText;

    [Header("Hero Footstep SFX")]
    public AudioClip walkSFX;
    public AudioClip runSFX;
    [Range(0.1f, 2.0f)] public float walkVolume = 1.5f;  // 🔹 Increased volume range (up to 2.0)
    [Range(0.1f, 2.0f)] public float runVolume = 1.5f;

    private int currentNode = 0;
    private bool isMoving = false;
    private Animator animator;
    private Coroutine movementCoroutine;
    private float currentSpeed = 0f;
    private AudioSource audioSource;

    public bool IsMoving => isMoving;
    public float CurrentSpeed => currentSpeed;
    public float WalkSpeed => walkSpeed;
    public float RunSpeed => runSpeed;
    public int GetCurrentNodeIndex() => currentNode;

    private void Start()
    {
        animator = hero.GetComponent<Animator>();

        // Scale the hero by 20%
        hero.localScale = hero.localScale * 1f;

        if (HasAnimatorParameter(animator, "State"))
            animator.SetInteger("State", 0);
        else
            Debug.LogError("Animator does not have a 'State' parameter! Check the Animator setup.");

        // Initialize audio source
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.spatialBlend = 1.0f; // 3D sound
    }

    public void MoveHero(int steps)
    {
        if (!isMoving && steps > 0 && pathNodes.Count > 0)
            movementCoroutine = StartCoroutine(MoveAlongPath(steps));
    }

    private IEnumerator MoveAlongPath(int steps)
    {
        isMoving = true;
        currentSpeed = walkSpeed;

        for (int i = 0; i < steps; i++)
        {
            if (currentNode >= pathNodes.Count) currentNode = 0;
            int nextNodeIndex = (currentNode + 1) % pathNodes.Count;
            Transform nextNodeTransform = pathNodes[nextNodeIndex];
            Vector3 targetPos = nextNodeTransform.position;

            float distance = Vector3.Distance(hero.position, targetPos);
            float targetSpeed = (distance > runDistanceThreshold) ? runSpeed : walkSpeed;
            int animationState = (targetSpeed == runSpeed) ? 2 : 1;

            if (HasAnimatorParameter(animator, "State"))
            {
                animator.SetInteger("State", animationState);
                animator.Update(0);
            }

            PlayFootstepSound(targetSpeed);

            yield return StartCoroutine(MoveAndTurnSmoothly(targetPos, targetSpeed));

            hero.position = targetPos;
            currentNode = nextNodeIndex;

            NodeProperty nodeProperty = nextNodeTransform.GetComponent<NodeProperty>();
            if (nodeProperty != null && nodeProperty.grantsGold)
            {
                EconManager.Instance.AddGold(nodeProperty.goldAmount);
                Debug.Log($"Gold Collected! Current Gold: {EconManager.Instance.CurrentGold}");
            }

            UpdateMovesLeftText(steps - (i + 1));
        }

        yield return new WaitForSeconds(0.1f);
        StopHero();
    }

    private IEnumerator MoveAndTurnSmoothly(Vector3 targetPosition, float targetSpeed)
    {
        while (Vector3.Distance(hero.position, targetPosition) > nodeStopThreshold)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, (currentSpeed < targetSpeed ? acceleration : deceleration) * Time.deltaTime);
            hero.position = Vector3.MoveTowards(hero.position, targetPosition, currentSpeed * Time.deltaTime);

            Vector3 direction = (targetPosition - hero.position).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                hero.rotation = Quaternion.Slerp(hero.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            yield return null;
        }
    }

    private void StopHero()
    {
        isMoving = false;
        currentSpeed = 0f;

        if (HasAnimatorParameter(animator, "State"))
        {
            animator.SetInteger("State", 0);
            animator.Update(0);
        }

        StopFootstepSound();
        UpdateMovesLeftText(0);
    }

    private void PlayFootstepSound(float speed)
    {
        if (audioSource == null) return;

        AudioClip selectedClip = (speed == runSpeed) ? runSFX : walkSFX;
        float volume = (speed == runSpeed) ? runVolume : walkVolume;

        if (selectedClip != null && audioSource.clip != selectedClip)
        {
            audioSource.clip = selectedClip;
            audioSource.volume = volume;
            audioSource.Play();
        }
    }

    private void StopFootstepSound()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    private void UpdateMovesLeftText(int movesLeft)
    {
        if (movesLeftText != null)
            movesLeftText.text = $"Moves Left: {movesLeft}";
    }

    private bool HasAnimatorParameter(Animator animator, string paramName)
    {
        foreach (AnimatorControllerParameter param in animator.parameters)
            if (param.name == paramName)
                return true;

        return false;
    }

    void OnDrawGizmos()
    {
        if (pathNodes == null || pathNodes.Count < 2) return;

        Gizmos.color = Color.cyan;

        for (int i = 0; i < pathNodes.Count - 1; i++)
            Gizmos.DrawLine(pathNodes[i].position, pathNodes[i + 1].position);

        Gizmos.DrawLine(pathNodes[pathNodes.Count - 1].position, pathNodes[0].position);
    }
}
