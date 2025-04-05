using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Camera Settings")]
    public Transform hero; // The hero to follow
    public Vector3 offset = new Vector3(0, 5, -7); // Default third-person view
    public float followSpeed = 5f; // Speed of camera movement
    public float rotationSpeed = 5f; // Smooth rotation behind hero
    public float zoomSpeed = 2f; // Speed of zoom effect
    public float minZoom = 5f; // Closest zoom
    public float maxZoom = 10f; // Farthest zoom

    [Header("Customization")]
    [Range(0f, 1f)] public float intensity = 0.7f; // Adjust how strongly the camera follows the hero
    public bool flipFollowDirection = false; // Toggle to flip follow direction manually

    [Header("Random Flip Settings")]
    public bool enableFlipChance = false; // Enables random flipping of follow direction at path nodes
    [Range(0f, 1f)] public float flipChance = 0.2f; // Probability of flipping direction when reaching a path node

    [Header("Side Offset Settings")]
    public bool autoSideOffset = false; // Enable automatic side offset based on movement direction
    [Range(-1f, 1f)] public float sideOffset = 0f; // Manual side offset

    private HeroPositionManager heroPositionManager;
    private float currentZoom;
    private int lastNodeIndex = -1; // Tracks the last node reached

    private void Start()
    {
        if (hero == null)
        {
            Debug.LogError("CameraFollow: No hero assigned! Please set the hero GameObject in the Inspector.");
            enabled = false;
            return;
        }

        heroPositionManager = hero.GetComponent<HeroPositionManager>();
        if (heroPositionManager == null)
        {
            Debug.LogError($"CameraFollow: Hero '{hero.name}' does not have HeroPositionManager! Ensure the script is attached.");
            enabled = false;
            return;
        }

        currentZoom = offset.magnitude; // Initialize zoom level
    }

    private void LateUpdate()
    {
        if (hero == null || heroPositionManager == null) return;

        // Flip direction only when the hero reaches a new path node
        CheckForNodeFlip();

        if (autoSideOffset)
        {
            UpdateSideOffsetAutomatically();
        }

        FollowHero();
        AdjustZoom();
    }

    private void CheckForNodeFlip()
    {
        if (!enableFlipChance) return; // If disabled, do nothing

        int currentNode = heroPositionManager.GetCurrentNodeIndex(); // Get hero's current node index
        if (currentNode != lastNodeIndex) // Only trigger if we've moved to a new node
        {
            lastNodeIndex = currentNode; // Update the last known node

            if (Random.value < flipChance) // Roll to see if we should flip
            {
                flipFollowDirection = !flipFollowDirection;
            }
        }
    }

    private void UpdateSideOffsetAutomatically()
    {
        Vector3 velocity = hero.forward * heroPositionManager.CurrentSpeed;
        float movementAngle = Vector3.SignedAngle(Vector3.forward, velocity, Vector3.up);

        if (movementAngle > 10f)
        {
            sideOffset = Mathf.Lerp(sideOffset, 1f, followSpeed * Time.deltaTime);
        }
        else if (movementAngle < -10f)
        {
            sideOffset = Mathf.Lerp(sideOffset, -1f, followSpeed * Time.deltaTime);
        }
        else
        {
            sideOffset = Mathf.Lerp(sideOffset, 0f, followSpeed * Time.deltaTime);
        }
    }

    private void FollowHero()
    {
        Vector3 heroForwardFlat = new Vector3(hero.forward.x, 0, hero.forward.z).normalized;
        Vector3 heroRightFlat = new Vector3(hero.right.x, 0, hero.right.z).normalized;

        float directionMultiplier = flipFollowDirection ? 1f : -1f;

        Vector3 targetPosition = hero.position
            + heroForwardFlat * offset.z * directionMultiplier
            + heroRightFlat * offset.z * sideOffset
            + Vector3.up * offset.y;

        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * intensity * Time.deltaTime);

        Quaternion targetRotation = Quaternion.LookRotation(hero.position - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * intensity * Time.deltaTime);
    }

    private void AdjustZoom()
    {
        float heroSpeed = heroPositionManager.WalkSpeed;
        if (heroPositionManager.IsMoving)
        {
            heroSpeed = heroPositionManager.CurrentSpeed;
        }

        float targetZoom = Mathf.Lerp(minZoom, maxZoom, heroSpeed / heroPositionManager.RunSpeed);
        currentZoom = Mathf.Lerp(currentZoom, targetZoom, zoomSpeed * intensity * Time.deltaTime);

        offset.z = -currentZoom;
    }
}
