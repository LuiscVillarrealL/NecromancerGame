using UnityEngine;

[RequireComponent(typeof(Unit))]
public class UnitCombat : MonoBehaviour
{

[Header("Attack")]
    [SerializeField, Min(1)] private int damage = 10;
    [SerializeField, Min(0.1f)] private float attackRange = 1.5f;
    [SerializeField, Min(0.1f)] private float attackCooldown = 1f;

    private Unit _unit;
    private Unit _target;
    private float _nextAttackTime;

    public Unit Target => _target;

    private void Awake()
    {
        _unit = GetComponent<Unit>();
    }

    private void Update()
{
    if (_unit.IsDead) return;

    // A target can die, disappear, or change faction.
    if (_target == null) return;

    if (_target.IsDead ||
        !_target.gameObject.activeInHierarchy ||
        !_unit.IsEnemy(_target))
    {
        ClearTarget();
        return;
    }

    // Ignore height when measuring ground distance.
    Vector3 direction = _target.transform.position - transform.position;
    direction.y = 0f;

    if (direction.sqrMagnitude > attackRange * attackRange)
        return;

    if (Time.time < _nextAttackTime)
        return;

    // Face the target before attacking.
    if (direction.sqrMagnitude > 0.001f)
        transform.rotation = Quaternion.LookRotation(direction);

    _nextAttackTime = Time.time + attackCooldown;
    _target.TakeDamage(damage);
}

    public void SetTarget(Unit target)
    {
        if (target == null || target.IsDead ||
            !target.gameObject.activeInHierarchy ||
            !_unit.IsEnemy(target))
        {
            return;
        }

        _target = target;
    }

    public void ClearTarget()
    {
        _target = null;
    }
}
