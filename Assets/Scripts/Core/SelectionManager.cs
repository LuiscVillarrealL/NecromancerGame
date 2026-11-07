using System.Collections.Generic;
using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    public static SelectionManager Instance { get; private set; }

    public IReadOnlyList<Unit> SelectedUnits => _selectedUnits;
    private readonly List<Unit> _selectedUnits = new();

    // Control groups: 0..9
    private readonly Dictionary<int, List<Unit>> _controlGroups = new();

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        // init groups
        for (int i = 0; i <= 9; i++)
            _controlGroups[i] = new List<Unit>();
    }

    public void ClearSelection()
    {
        foreach (var u in _selectedUnits)
            if (u) u.SetSelected(false);

        _selectedUnits.Clear();
    }

    public void AddToSelection(Unit unit)
    {
        if (!unit || !unit.IsPlayerControlled || unit.IsDead || !unit.gameObject.activeInHierarchy) return;
        if (_selectedUnits.Contains(unit)) return;

        _selectedUnits.Add(unit);
        unit.SetSelected(true);
    }

    public void RemoveFromSelection(Unit unit)
    {
        if (!unit) return;
        if (!_selectedUnits.Contains(unit)) return;

        _selectedUnits.Remove(unit);
        unit.SetSelected(false);
    }

    public void ToggleSelection(Unit unit)
    {
        if (!unit) return;

        if (_selectedUnits.Contains(unit)) RemoveFromSelection(unit);
        else AddToSelection(unit);
    }

    public void RemoveUnit(Unit unit)
    {
        RemoveFromSelection(unit);
        foreach (var group in _controlGroups.Values)
            group.Remove(unit);
    }

    public void SetSelection(IEnumerable<Unit> units)
    {
        ClearSelection();
        foreach (var u in units)
            AddToSelection(u);
    }

    // Control groups
    public void AssignGroup(int number)
    {
        _controlGroups[number].Clear();
        foreach (var u in _selectedUnits)
            if (u) _controlGroups[number].Add(u);
    }

    public void RecallGroup(int number)
    {
        // Clean nulls (destroyed units)
        _controlGroups[number].RemoveAll(u => u == null);

        SetSelection(_controlGroups[number]);
    }
}
