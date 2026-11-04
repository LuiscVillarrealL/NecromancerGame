using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Unit), typeof(UnitMovement), typeof(NavMeshAgent))]
public class PetMovement : MonoBehaviour
{
    public enum Order { Follow, Move, Hold }

    [SerializeField] private Transform leader;
    [Tooltip("Offset relative to the followed unit's facing. Negative Z stays behind it.")]
    [SerializeField] private Vector3 followOffset = new Vector3(0f, 0f, -2f);
    [SerializeField, Min(0.1f)] private float followDistance = 0.75f;
    [SerializeField, Min(0.1f)] private float repathInterval = 0.25f;

    public Order CurrentOrder { get; private set; } = Order.Follow;
    public Transform FollowTarget => leader;
    private Unit _unit;
    private NavMeshAgent _agent;
    private float _nextRepath;
    private bool _followingPath;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _unit = GetComponent<Unit>();
    }

    void Start()
    {
        if (leader == null)
        {
            var character = FindAnyObjectByType<MainCharacterMovement>();
            if (character != null && _unit.IsAlly(character.GetComponent<Unit>())) leader = character.transform;
        }
    }

    void Update()
    {
        if (!_agent.isActiveAndEnabled || !_agent.isOnNavMesh) return;

        if (CurrentOrder == Order.Move)
        {
            if (!_agent.pathPending && _agent.remainingDistance <= Mathf.Max(_agent.stoppingDistance, 0.15f))
                Hold();
            return;
        }

        if (CurrentOrder != Order.Follow) return;
        if (leader == null || !leader.gameObject.activeInHierarchy || !_unit.IsAlly(leader.GetComponent<Unit>()))
        {
            Hold();
            return;
        }
        if (Time.time < _nextRepath) return;
        _nextRepath = Time.time + repathInterval;

        Vector3 target = GetFollowPosition();
        Vector3 delta = target - transform.position;
        delta.y = 0f;
        // A wider restart distance prevents repeatedly stopping and starting at the boundary.
        float threshold = _followingPath ? followDistance : followDistance + 0.4f;
        if (delta.sqrMagnitude <= threshold * threshold)
        {
            if (_agent.hasPath || _agent.pathPending) _agent.ResetPath();
            _followingPath = false;
            return;
        }

        if (!_agent.hasPath || (target - _agent.destination).sqrMagnitude > 0.0625f)
            _followingPath = TrySetDestination(target);
    }

    private Vector3 GetFollowPosition()
    {
        Vector3 forward = Vector3.ProjectOnPlane(leader.forward, Vector3.up).normalized;
        if (forward.sqrMagnitude < 0.001f) forward = Vector3.forward;
        Vector3 right = Vector3.Cross(Vector3.up, forward);
        return leader.position + right * followOffset.x + Vector3.up * followOffset.y + forward * followOffset.z;
    }

    public void Follow()
    {
        var character = FindAnyObjectByType<MainCharacterMovement>();
        if (character != null) Follow(character.GetComponent<Unit>());
    }

    public bool Follow(Unit target)
    {
        if (target == null || target == _unit || !target.gameObject.activeInHierarchy || !_unit.IsAlly(target))
            return false;

        // Reject chains that would lead back to this pet or already contain a cycle.
        var visited = new System.Collections.Generic.HashSet<PetMovement>();
        PetMovement next = target.GetComponent<PetMovement>();
        while (next != null)
        {
            if (next == this || !visited.Add(next)) return false;
            if (next.CurrentOrder != Order.Follow || next.leader == null) break;
            next = next.leader.GetComponent<PetMovement>();
        }

        leader = target.transform;
        if (_agent.isActiveAndEnabled && _agent.isOnNavMesh) _agent.ResetPath();
        CurrentOrder = Order.Follow;
        _followingPath = false;
        _nextRepath = 0f;
        return true;
    }

    public void Hold()
    {
        CurrentOrder = Order.Hold;
        if (_agent.isActiveAndEnabled && _agent.isOnNavMesh) _agent.ResetPath();
    }

    public void MoveTo(Vector3 destination)
    {
        if (TrySetDestination(destination)) CurrentOrder = Order.Move;
    }

    private bool TrySetDestination(Vector3 destination)
    {
        if (!_agent.isActiveAndEnabled || !_agent.isOnNavMesh) return false;
        var filter = new NavMeshQueryFilter { agentTypeID = _agent.agentTypeID, areaMask = _agent.areaMask };
        if (!NavMesh.SamplePosition(destination, out NavMeshHit hit, 2f, filter)) return false;
        _agent.isStopped = false;
        return _agent.SetDestination(hit.position);
    }
}
