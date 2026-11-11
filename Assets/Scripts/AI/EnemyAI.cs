using UnityEngine;

[RequireComponent(typeof(Unit), typeof(UnitOrders), typeof(UnitCombat))]
[RequireComponent(typeof(UnitVision))]
// Cancel lost targets before movement and combat update this frame.
[DefaultExecutionOrder(-10)]
public class EnemyAI : MonoBehaviour
{
    [Header("Lost Target Search")]
    [SerializeField, Min(0f)] private float searchDuration = 2f;
    [SerializeField, Min(0.1f)] private float investigationTimeout = 10f;

    private Unit _unit;
    private UnitOrders _orders;
    private UnitCombat _combat;
    private UnitVision _vision;
    private Unit _rememberedTarget;
    private Vector3 _lastSeenPosition;
    private bool _investigating;
    private float _investigationDeadline;
    private float _searchDeadline = -1f;

    private void Awake()
    {
        _unit = GetComponent<Unit>();
        _orders = GetComponent<UnitOrders>();
        _combat = GetComponent<UnitCombat>();
        _vision = GetComponent<UnitVision>();
    }

    private void OnDisable()
    {
        _rememberedTarget = null;
        _investigating = false;
        _searchDeadline = -1f;
    }

    private void Update()
    {
        if (_unit.IsDead || _unit.IsPlayerControlled) return;

        if (_orders.CurrentOrder == UnitOrders.Order.Attack)
        {
            Unit target = _combat.Target;
            if (!IsValidTarget(target)) { ForgetTarget(); return; }
            if (_vision.CanSee(target))
            {
                RememberTarget(target);
                return;
            }

            // No observed position means there is nowhere reliable to investigate.
            if (_rememberedTarget != target) { ForgetTarget(); return; }
            _orders.Hold();
            _orders.MoveTo(_lastSeenPosition);
            _investigating = true;
            _investigationDeadline = Time.time + investigationTimeout;
            _searchDeadline = -1f;
        }

        if (!_investigating) return;
        if (!IsValidTarget(_rememberedTarget)) { ForgetTarget(); return; }

        // Visibility may be tested, but hidden positions never update our destination.
        if (_vision.CanSee(_rememberedTarget))
        {
            _orders.Attack(_rememberedTarget);
            RememberTarget(_rememberedTarget);
            return;
        }

        if (Time.time >= _investigationDeadline) { ForgetTarget(); return; }

        // Move orders become Hold on arrival (or remain Hold if no path was found).
        if (_orders.CurrentOrder == UnitOrders.Order.Hold)
        {
            if (_searchDeadline < 0f) _searchDeadline = Time.time + searchDuration;
            if (Time.time >= _searchDeadline) ForgetTarget();
        }
    }

    private bool IsValidTarget(Unit target)
    {
        return target != null && !target.IsDead && target.gameObject.activeInHierarchy &&
            _unit.IsEnemy(target);
    }

    private void RememberTarget(Unit target)
    {
        _rememberedTarget = target;
        _lastSeenPosition = target.transform.position;
        _investigating = false;
        _searchDeadline = -1f;
    }

    private void ForgetTarget()
    {
        _orders.Hold();
        _rememberedTarget = null;
        _investigating = false;
        _searchDeadline = -1f;
    }
}
