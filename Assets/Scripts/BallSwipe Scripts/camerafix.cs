using UnityEngine;

[ExecuteAlways] // Works both in Play Mode and Edit Mode
public class CameraAutoScaler : MonoBehaviour
{
    [Header("Reference Settings")]
    [Tooltip("Reference aspect ratio (e.g., 9:16 for portrait or 16:9 for landscape).")]
    public Vector2 referenceResolution = new Vector2(1080f, 1920f);

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("CameraAutoScaler: No Camera component found!");
            return;
        }

        AdjustCamera();
    }

#if UNITY_EDITOR
    void Update()
    {
        // Continuously update in Editor to preview scaling
        AdjustCamera();
    }
#endif

    void AdjustCamera()
    {
        if (cam == null) return;

        float targetAspect = referenceResolution.x / referenceResolution.y;
        float windowAspect = (float)Screen.width / Screen.height;

        float scaleFactor = targetAspect / windowAspect;

        // Adjust orthographic size based on aspect difference
        if (scaleFactor < 1f)
        {
            // Wider screen (e.g., tablet) — zoom out
            cam.orthographicSize = 5f / scaleFactor;
        }
        else
        {
            // Taller or same — use base size
            cam.orthographicSize = 5f;
        }
    }
}
