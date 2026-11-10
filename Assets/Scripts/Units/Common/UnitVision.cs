using UnityEngine;

public class UnitVision : MonoBehaviour
{

    [Header("Vision Debug")]
    [SerializeField, Min(0.1f)] private float visionRange = 8f;
    [SerializeField, Range(1f, 360f)] private float visionAngle = 90f;
    [SerializeField] private Color visionColor = Color.yellow;


    private void OnDrawGizmos()
    {
        Gizmos.color = visionColor;

        // Slightly above the ground to keep the lines visible.
        Vector3 origin = transform.position + Vector3.up * 0.1f;

        Vector3 forward = Vector3.ProjectOnPlane(
            transform.forward, Vector3.up).normalized;

        if (forward.sqrMagnitude < 0.001f)
            forward = Vector3.forward;

        const int segments = 32;

        Vector3 leftDirection =
            Quaternion.AngleAxis(-visionAngle * 0.5f, Vector3.up)
            * forward;

        Vector3 previousPoint = origin + leftDirection * visionRange;

        Gizmos.DrawLine(origin, previousPoint);

        for (int i = 1; i <= segments; i++)
        {
            float angle =
                -visionAngle * 0.5f + visionAngle * i / segments;

            Vector3 direction =
                Quaternion.AngleAxis(angle, Vector3.up) * forward;

            Vector3 point = origin + direction * visionRange;

            Gizmos.DrawLine(previousPoint, point);
            previousPoint = point;
        }

        Gizmos.DrawLine(origin, previousPoint);
    }
}
