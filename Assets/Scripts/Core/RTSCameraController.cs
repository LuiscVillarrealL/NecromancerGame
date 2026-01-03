using UnityEngine;

public class RTSCameraController : MonoBehaviour
{
    [Header("Pan")]
    public float panSpeed = 20f;

    [Header("Zoom (Orthographic Size)")]
    public Camera cam;
    public float zoomSpeed = 5f;
    public float minZoom = 5f;
    public float maxZoom = 20f;

    [Header("Bounds (Optional)")]
    public bool useBounds = false;
    public Vector2 minBounds = new Vector2(-50, -50);
    public Vector2 maxBounds = new Vector2(50, 50);

    void Reset()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (cam == null) return;

        // Pan with WASD / Arrow keys
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // Move relative to camera rig's facing (keeps pan feeling correct in isometric)
        Vector3 right = transform.right;
        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;

        Vector3 move = (right * h + forward * v).normalized;
        transform.position += move * panSpeed * Time.deltaTime;

        // Zoom with mouse wheel
        float scroll = Input.mouseScrollDelta.y;
        if (scroll != 0)
        {
            cam.orthographicSize = Mathf.Clamp(
                cam.orthographicSize - scroll * zoomSpeed,
                minZoom,
                maxZoom
            );
        }

        // Optional clamp to map bounds (X/Z)
        if (useBounds)
        {
            Vector3 p = transform.position;
            p.x = Mathf.Clamp(p.x, minBounds.x, maxBounds.x);
            p.z = Mathf.Clamp(p.z, minBounds.y, maxBounds.y);
            transform.position = p;
        }
    }
}