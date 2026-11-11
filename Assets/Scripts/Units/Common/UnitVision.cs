using UnityEngine;

public class UnitVision : MonoBehaviour
{

    [Header("Vision Debug")]
    [SerializeField, Min(0.1f)] private float visionRange = 8f;
    [SerializeField, Range(1f, 360f)] private float visionAngle = 90f;
    [SerializeField] private Color visionColor = Color.yellow;
    [Header("Line of Sight")]
    [SerializeField] private LayerMask obstacleMask = 1 << 8;
    [SerializeField, Min(0f)] private float eyeHeight = 1f;

    public Unit FindNearestVisibleEnemy(Unit observer)
    {
        if (observer == null || observer.IsDead || !isActiveAndEnabled) return null;

        Unit nearest = null;
        float nearestDistance = float.PositiveInfinity;
        foreach (Unit candidate in FindObjectsByType<Unit>())
        {
            if (candidate == observer || !observer.IsEnemy(candidate)) continue;
            Vector3 delta = candidate.transform.position - transform.position;
            delta.y = 0f;
            float distance = delta.sqrMagnitude;
            if (distance >= nearestDistance || !CanSee(candidate)) continue;
            nearest = candidate;
            nearestDistance = distance;
        }
        return nearest;
    }

    public bool CanSee(Unit target)
    {
        if (!isActiveAndEnabled || target == null || target.IsDead ||
            !target.gameObject.activeInHierarchy) return false;

        Vector3 direction = target.transform.position - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude > visionRange * visionRange) return false;

        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        if (forward.sqrMagnitude < 0.001f) forward = Vector3.forward;
        if (direction.sqrMagnitude > 0.000001f &&
            Vector3.Angle(forward, direction) > visionAngle * 0.5f) return false;

        Vector3 eyes = transform.position + Vector3.up * eyeHeight;
        Vector3 targetPoint = target.transform.position + Vector3.up * eyeHeight;
        return !Physics.Linecast(eyes, targetPoint, obstacleMask, QueryTriggerInteraction.Ignore);
    }

    private Vector3 GetVisionEdge(Vector3 origin, Vector3 direction)
    {
        if (Physics.Raycast(origin, direction, out RaycastHit hit, visionRange,
            obstacleMask, QueryTriggerInteraction.Ignore)) return hit.point;
        return origin + direction * visionRange;
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = visionColor;

        Vector3 origin = transform.position + Vector3.up * eyeHeight;

        Vector3 forward = Vector3.ProjectOnPlane(
            transform.forward, Vector3.up).normalized;

        if (forward.sqrMagnitude < 0.001f)
            forward = Vector3.forward;

        const int segments = 32;

        Vector3 leftDirection =
            Quaternion.AngleAxis(-visionAngle * 0.5f, Vector3.up)
            * forward;

        Vector3 previousPoint = GetVisionEdge(origin, leftDirection);

        Gizmos.DrawLine(origin, previousPoint);

        for (int i = 1; i <= segments; i++)
        {
            float angle =
                -visionAngle * 0.5f + visionAngle * i / segments;

            Vector3 direction =
                Quaternion.AngleAxis(angle, Vector3.up) * forward;

            Vector3 point = GetVisionEdge(origin, direction);

            Gizmos.DrawLine(previousPoint, point);
            previousPoint = point;
        }

        Gizmos.DrawLine(origin, previousPoint);
    }
}
