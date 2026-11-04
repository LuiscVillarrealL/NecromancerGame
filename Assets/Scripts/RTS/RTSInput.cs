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

        if (Input.GetKeyDown(KeyCode.G) || Input.GetKeyDown(KeyCode.H))
        {
            foreach (Unit unit in SelectionManager.Instance.SelectedUnits)
            {
                if (unit == null || !unit.IsPlayerControlled) continue;
                PetMovement pet = unit.GetComponent<PetMovement>();
                if (pet == null) continue;
                if (Input.GetKeyDown(KeyCode.H)) pet.Hold();
                else pet.Follow();
            }
        }

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
        if (Camera.main == null) return;

        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // Pet colliders may be on child objects such as Body.
                Unit target = hit.collider.GetComponentInParent<Unit>();
                if (target != null)
                {
                    foreach (Unit unit in SelectionManager.Instance.SelectedUnits)
                    {
                        if (unit == null || !unit.IsPlayerControlled || !unit.IsAlly(target)) continue;
                        PetMovement pet = unit.GetComponent<PetMovement>();
                        if (pet != null) pet.Follow(target);
                    }
                    // Enemy/neutral targets are reserved for combat and interaction.
                    return;
                }

                int petCount = 0;
                foreach (Unit unit in SelectionManager.Instance.SelectedUnits)
                    if (unit != null && unit.IsPlayerControlled && unit.GetComponent<PetMovement>() != null) petCount++;

                int petIndex = 0;
                foreach (Unit unit in SelectionManager.Instance.SelectedUnits)
                {
                    if (unit == null || !unit.IsPlayerControlled) continue;
                    if (unit.GetComponent<MainCharacterMovement>() != null)
                        continue;

                    PetMovement pet = unit.GetComponent<PetMovement>();
                    if (pet != null)
                    {
                        // Spread group destinations rather than stacking every pet on one point.
                        Vector3 offset = Vector3.zero;
                        if (petCount > 1)
                        {
                            float angle = petIndex * Mathf.PI * 2f / petCount;
                            float radius = Mathf.Max(1f, petCount * 0.3f);
                            offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
                        }
                        pet.MoveTo(hit.point + offset);
                        petIndex++;
                    }
                    else if (unit.Movement != null) unit.Movement.MoveTo(hit.point);
                }
            }
        }
    }
}
