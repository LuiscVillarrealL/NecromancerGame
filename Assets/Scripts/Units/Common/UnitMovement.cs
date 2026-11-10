using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(NavMeshAgent))]
public class UnitMovement : MonoBehaviour
{
    private NavMeshAgent agent;
    public bool IsReady => agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh;
    public bool HasArrived => IsReady && !agent.pathPending &&
        (!agent.hasPath || agent.remainingDistance <= Mathf.Max(agent.stoppingDistance, 0.15f));

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void MoveTo(Vector3 destination)
    {
        TryMoveTo(destination);
    }

    public bool TryMoveTo(Vector3 destination)
    {
        if (!IsReady) return false;
        var filter = new NavMeshQueryFilter { agentTypeID = agent.agentTypeID, areaMask = agent.areaMask };
        if (!NavMesh.SamplePosition(destination, out NavMeshHit hit, 2f, filter)) return false;
        var path = new NavMeshPath();
        if (!agent.CalculatePath(hit.position, path) || path.status != NavMeshPathStatus.PathComplete)
            return false;
        agent.isStopped = false;
        return agent.SetPath(path);
    }

    public void Stop()
    {
        if (IsReady) agent.ResetPath();
    }

    public float Speed => agent.velocity.magnitude;
}
