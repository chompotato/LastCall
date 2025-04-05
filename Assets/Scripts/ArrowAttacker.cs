using UnityEngine;

public class ArrowAttacker : MonoBehaviour
{
    public int clickDamage = 10;
    public AudioClip hitSFX;
    public GameObject hitVFX;
    public float vfxOffset = 0.5f; // Offset to prevent clipping with ground

    private Camera mainCamera;
    private AudioSource audioSource;

    void Start()
    {
        mainCamera = Camera.main;
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleClick();
        }
    }

    void HandleClick()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Enemy enemy = hit.collider.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.UseStamina(clickDamage);
                if (hitSFX) audioSource.PlayOneShot(hitSFX);

                if (hitVFX)
                {
                    Vector3 vfxPosition = hit.point + Vector3.up * vfxOffset;
                    GameObject vfxInstance = Instantiate(hitVFX, vfxPosition, Quaternion.identity);

                    // Ensure it renders above everything
                    Renderer vfxRenderer = vfxInstance.GetComponent<Renderer>();
                    if (vfxRenderer)
                    {
                        vfxRenderer.sortingOrder = 1000; // High value to render on top
                    }

                    Destroy(vfxInstance, 1.5f);
                }
            }
        }
    }
}
