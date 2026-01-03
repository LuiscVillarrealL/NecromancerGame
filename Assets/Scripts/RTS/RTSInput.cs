using UnityEngine;

public class RTSInput : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                foreach (Unit unit in SelectionManager.Instance.SelectedUnits)
                {
                    unit.Movement.MoveTo(hit.point);
                }
            }
        }
    }
}