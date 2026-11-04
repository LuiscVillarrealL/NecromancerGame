using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Inventory))]
public class MainStationInteractor : MonoBehaviour
{
    private Inventory _mainInventory;

    void Awake()
    {
        _mainInventory = GetComponent<Inventory>();
    }

    void Update()
    {
        // ignore clicks on UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (!Input.GetMouseButtonDown(0))
            return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, 500f))
            return;

        Station station = hit.collider.GetComponentInParent<Station>();
        if (station == null)
            return;

        float dist = Vector3.Distance(transform.position, station.transform.position);
        if (dist > station.interactRange)
        {
            Debug.Log($"Too far from {station.stationName} (need {station.interactRange:0.0})");
            return;
        }

        InventoryUI.Instance.Open(_mainInventory, station.Inventory, station.stationName);
    }
}