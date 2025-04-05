using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    public TextMeshProUGUI textMesh;
    public float floatSpeed = 1f;
    public float fadeDuration = 1.5f;

    private Color startColor;
    private float timer = 0f;
    private Transform mainCamera;

    public void Setup(string message, Color color)
    {
        textMesh.text = message;
        startColor = color;
        textMesh.color = startColor;

        mainCamera = Camera.main.transform; // Get main camera reference
    }

    private void Update()
    {
        transform.position += Vector3.up * floatSpeed * Time.deltaTime; // Move upward

        // Always face the camera
        if (mainCamera != null)
        {
            transform.LookAt(mainCamera);
            transform.rotation = Quaternion.LookRotation(mainCamera.forward); // Ensures proper orientation
        }

        timer += Time.deltaTime;
        if (timer >= fadeDuration)
        {
            Destroy(gameObject); // Destroy after fade
        }
        else
        {
            float fadeAlpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            textMesh.color = new Color(startColor.r, startColor.g, startColor.b, fadeAlpha);
        }
    }
}
