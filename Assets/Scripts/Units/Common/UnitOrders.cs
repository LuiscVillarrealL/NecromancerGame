using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Unit), typeof(UnitMovement))]
public class UnitOrders : MonoBehaviour
{
    public enum Order { Hold, Move, Follow, Attack }

    [SerializeField] private Transform leader;
    [SerializeField] private Vector3 followOffset = new Vector3(0f, 0f, -2f);
    [SerializeField, Min(0.1f)] private float followDistance = 0.75f;
    [SerializeField, Min(0.1f)] private float repathInterval = 0.25f;
    [SerializeField, Min(0.1f)] private float targetScanInterval = 0.25f;

    public Order CurrentOrder { get; private set; } = Order.Hold;
    public Transform FollowTarget => leader;
    private Unit _unit;
    private UnitMovement _movement;
    private UnitCombat _combat;
    private float _nextRepath;
    private float _nextTargetScan;
    private UnitVision _vision;

    private void Awake()
    {
        _unit = GetComponent<Unit>();
        _movement = GetComponent<UnitMovement>();
        _combat = GetComponent<UnitCombat>();
        _vision = GetComponent<UnitVision>();
    }

    private void Start()
    {
        if (leader != null) Follow(leader.GetComponent<Unit>());
    }

    private void OnEnable()
    {
        _unit.Attacked += HandleAttacked;
    }

    private void HandleAttacked(Unit attacker)
    {
        // Hold is also the idle state. Active orders take priority over retaliation.
        if (_unit.IsDead || CurrentOrder != Order.Hold ||
            !IsValid(attacker) || !_unit.IsEnemy(attacker)) return;

        Vector3 direction = attacker.transform.position - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(direction);

        UnitVision vision = GetComponent<UnitVision>();
        if (vision != null && vision.CanSee(attacker)) Attack(attacker);
    }

    public void Hold()
    {
        CurrentOrder = Order.Hold;
        leader = null;
        if (_combat != null) _combat.ClearTarget();
        _movement.Stop();
    }

    public void MoveTo(Vector3 destination)
    {
        if (_unit.IsDead || !_movement.TryMoveTo(destination)) return;
        if (_combat != null) _combat.ClearTarget();
        leader = null;
        CurrentOrder = Order.Move;
    }

    public bool Follow(Unit target)
    {
        if (_unit.IsDead || !IsValid(target) || target == _unit || !_unit.IsAlly(target)) return false;
        var visited = new HashSet<UnitOrders>();
        UnitOrders next = target.GetComponent<UnitOrders>();
        while (next != null)
        {
            if (next == this || !visited.Add(next)) return false;
            if (next.CurrentOrder != Order.Follow || next.leader == null) break;
            next = next.leader.GetComponent<UnitOrders>();
        }
        Hold();
        leader = target.transform;
        CurrentOrder = Order.Follow;
        _nextRepath = 0f;
        return true;
    }

    public void Attack(Unit target)
    {
        if (_unit.IsDead || _combat == null || !_combat.isActiveAndEnabled ||
            !IsValid(target) || !_unit.IsEnemy(target)) return;
        Hold();
        _combat.SetTarget(target);
        CurrentOrder = Order.Attack;
        _nextRepath = 0f;
    }

    private static bool IsValid(Unit target)
    {
        return target != null && !target.IsDead && target.gameObject.activeInHierarchy;
    }

    private void Update()
    {
        if (_unit.IsDead) return;
        if (CurrentOrder == Order.Hold && _combat != null && _combat.isActiveAndEnabled &&
            _vision != null && Time.time >= _nextTargetScan)
        {
            _nextTargetScan = Time.time + targetScanInterval;
            Unit target = _vision.FindNearestVisibleEnemy(_unit);
            if (target != null) Attack(target);
        }
        if (CurrentOrder == Order.Move)
        {
            if (_movement.HasArrived) Hold();
            return;
        }

        Vector3 destination;
        float stopDistance;
        if (CurrentOrder == Order.Follow)
        {
            Unit target = leader != null ? leader.GetComponent<Unit>() : null;
            if (!IsValid(target) || !_unit.IsAlly(target)) { Hold(); return; }
            destination = leader.position + leader.rotation * followOffset;
            stopDistance = followDistance;
        }
        else if (CurrentOrder == Order.Attack)
        {
            if (_combat == null || !_combat.isActiveAndEnabled ||
                !IsValid(_combat.Target) || !_unit.IsEnemy(_combat.Target)) { Hold(); return; }
            destination = _combat.Target.transform.position;
            // Aim inside the damage range so navigation stopping distance doesn't prevent attacks.
            stopDistance = _combat.AttackRange * 0.9f;
        }
        else return;

        Vector3 delta = destination - transform.position;
        delta.y = 0f;
        if (delta.sqrMagnitude <= stopDistance * stopDistance)
        {
            _movement.Stop();
            return;
        }
        if (Time.time < _nextRepath) return;
        _nextRepath = Time.time + repathInterval;
        if (CurrentOrder == Order.Attack)
            destination -= delta.normalized * stopDistance;
        _movement.TryMoveTo(destination);
    }

    private void OnDisable()
    {
        if (_unit != null) _unit.Attacked -= HandleAttacked;
        if (_movement != null) Hold();
    }
}
