using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class RTSInput : MonoBehaviour
{
    void Update()
    {
        if (SelectionManager.Instance == null) return;
        if (InventoryUI.Instance != null && InventoryUI.Instance.inventoryWindow != null &&
            InventoryUI.Instance.inventoryWindow.activeInHierarchy) return;
        GameObject focused = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
        if (focused != null && (focused.GetComponent<InputField>() != null ||
                                focused.GetComponent<TMP_InputField>() != null)) return;

        if (Input.GetKeyDown(KeyCode.H))
            foreach (Unit unit in SelectionManager.Instance.SelectedUnits)
                if (CanCommand(unit)) unit.GetComponent<UnitOrders>().Hold();

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
        if (Camera.main == null || !Input.GetMouseButtonDown(1)) return;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, 500f)) return;
        Unit target = hit.collider.GetComponentInParent<Unit>();
        if (target != null)
        {
            foreach (Unit unit in SelectionManager.Instance.SelectedUnits)
            {
                if (!CanCommand(unit)) continue;
                UnitOrders orders = unit.GetComponent<UnitOrders>();
                if (unit.IsEnemy(target)) orders.Attack(target);
                else if (unit.IsAlly(target)) orders.Follow(target);
            }
            return;
        }
        // Stations keep their existing left-click interaction.
        if (hit.collider.GetComponentInParent<Station>() != null) return;

        int count = 0;
        foreach (Unit unit in SelectionManager.Instance.SelectedUnits)
            if (CanCommand(unit)) count++;
        int index = 0;
        foreach (Unit unit in SelectionManager.Instance.SelectedUnits)
        {
            if (!CanCommand(unit)) continue;
            Vector3 offset = Vector3.zero;
            if (count > 1)
            {
                float angle = index * Mathf.PI * 2f / count;
                float radius = Mathf.Max(1f, count * 0.3f);
                offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
            }
            unit.GetComponent<UnitOrders>().MoveTo(hit.point + offset);
            index++;
        }
    }

    private static bool CanCommand(Unit unit)
    {
        return unit != null && unit.IsPlayerControlled && !unit.IsDead &&
            unit.gameObject.activeInHierarchy &&
            unit.TryGetComponent<UnitOrders>(out var orders) && orders.isActiveAndEnabled;
    }
}
