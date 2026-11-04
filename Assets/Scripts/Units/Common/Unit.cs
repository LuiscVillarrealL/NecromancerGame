using UnityEngine;

public class Unit : MonoBehaviour
{
    public enum UnitFaction { Player, Enemy, Neutral }

    [SerializeField] private UnitFaction faction = UnitFaction.Player;
    [SerializeField] private GameObject selectionVisual;

    public UnitFaction Faction => faction;
    public bool IsPlayerControlled => faction == UnitFaction.Player;
    public UnitMovement Movement { get; private set; }

    public bool IsAlly(Unit other)
    {
        return other != null && faction != UnitFaction.Neutral && faction == other.faction;
    }

    public bool IsEnemy(Unit other)
    {
        return other != null && faction != UnitFaction.Neutral &&
               other.faction != UnitFaction.Neutral && faction != other.faction;
    }

    void Awake()
    {
        Movement = GetComponent<UnitMovement>();
    }

    public void SetSelected(bool selected)
    {
        if (selectionVisual != null)
            selectionVisual.SetActive(selected);
    }
}
