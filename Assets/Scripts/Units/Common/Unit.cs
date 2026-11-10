using UnityEngine;
using static FactionRelations;

[RequireComponent(typeof(UnitVision))]

public class Unit : MonoBehaviour
{
    public enum UnitFaction { Player, Enemy, Neutral }

    [SerializeField] private FactionDefinition faction;
    [SerializeField] private FactionRelations factionRelations;
    [SerializeField] private bool isPlayerControlled;
    [SerializeField] private GameObject selectionVisual;

    [Header("Max Health")]
    [SerializeField, Min(1)] private int maxHealth = 100;

    public int MaxHealth => maxHealth;
    [field: SerializeField]
    [field: Tooltip("Runtime health. Initialized to Max Health when the unit awakens. Use TakeDamage or Heal to trigger health events.")]
    public int CurrentHealth { get; private set; }
    public bool IsDead => CurrentHealth <= 0;
    public event System.Action<int, int> HealthChanged;
    public event System.Action Died;

public FactionDefinition Faction => faction;
public bool IsPlayerControlled => isPlayerControlled;
    public UnitMovement Movement { get; private set; }

  public bool IsAlly(Unit other)
{
    if (other == null || factionRelations == null)
        return false;

    return factionRelations.GetRelationship(faction, other.Faction)
        == FactionRelationship.Allied;
}

public bool IsEnemy(Unit other)
{
    if (other == null || factionRelations == null)
        return false;

    return factionRelations.GetRelationship(faction, other.Faction)
        == FactionRelationship.Hostile;
}

    void Awake()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        CurrentHealth = maxHealth;
        Movement = GetComponent<UnitMovement>();
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || IsDead) return;

        CurrentHealth -= Mathf.Min(amount, CurrentHealth);
        bool died = IsDead;
        HealthChanged?.Invoke(CurrentHealth, maxHealth);
        if (died)
        {
            Die(died);
        }



    }

    private void Die(bool died)
    {
        if (SelectionManager.Instance != null)
            SelectionManager.Instance.RemoveUnit(this);
        SetSelected(false);
        Debug.Log("This unit died!");
        if (died) Died?.Invoke();
        gameObject.SetActive(false);
    }

    public void Heal(int amount)
    {
        if (amount <= 0 || IsDead) return;

        int restored = Mathf.Min(amount, maxHealth - CurrentHealth);
        if (restored <= 0) return;
        CurrentHealth += restored;
        HealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    void OnValidate()
    {
        maxHealth = Mathf.Max(1, maxHealth);
    }

    public void SetSelected(bool selected)
    {
        if (selectionVisual != null)
            selectionVisual.SetActive(selected);
    }
}
