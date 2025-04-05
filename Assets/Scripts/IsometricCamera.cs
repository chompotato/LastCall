using UnityEngine;

[ExecuteInEditMode]
public class IsometricCamera : MonoBehaviour
{
    public Transform target;
    public float cameraSize = 10f;
    public Vector3 cameraOffset = new Vector3(-10f, 10f, -10f);

    private Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam)
        {
            cam.orthographic = true;
            cam.orthographicSize = cameraSize;
            transform.rotation = Quaternion.Euler(30, 45, 0);
        }
    }

    void LateUpdate()
    {
        if (target)
        {
            transform.position = target.position + cameraOffset;
        }

        if (!Application.isPlaying)
        {
            cam.orthographicSize = cameraSize;
            transform.rotation = Quaternion.Euler(30, 45, 0);
            if (target)
                transform.position = target.position + cameraOffset;
        }
    }
}