using System.Collections.Generic;
using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    public static SelectionManager Instance { get; private set; }

    public List<Unit> SelectedUnits { get; private set; } = new();

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Select(Unit unit)
    {
        if (SelectedUnits.Contains(unit))
            return;

        SelectedUnits.Add(unit);
        unit.SetSelected(true);
    }

    public void Deselect(Unit unit)
    {
        if (!SelectedUnits.Contains(unit))
            return;

        SelectedUnits.Remove(unit);
        unit.SetSelected(false);
    }

    public void ClearSelection()
    {
        foreach (var unit in SelectedUnits)
            unit.SetSelected(false);

        SelectedUnits.Clear();
    }
}