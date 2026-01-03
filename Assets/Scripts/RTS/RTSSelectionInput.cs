using UnityEngine;

public class RTSSelectionInput : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SelectionManager.Instance.ClearSelection();

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Unit unit = hit.collider.GetComponentInParent<Unit>();
                if (unit != null)
                {
                    SelectionManager.Instance.Select(unit);
                }
            }
        }
    }
}