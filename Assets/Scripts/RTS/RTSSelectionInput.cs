using UnityEngine;
using UnityEngine.EventSystems;

public class RTSSelectionInput : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private RectTransform selectionBox; // UI Image RectTransform
    [SerializeField] private Canvas canvas;              // Screen Space Overlay canvas

    [Header("Selection")]
    [SerializeField] private LayerMask unitMask = ~0;    // optionally make a "Units" layer
    [SerializeField] private float clickDragThreshold = 10f;

    private Vector2 _dragStartScreen;
    private bool _dragging;

    void Start()
    {
        if (canvas == null) canvas = FindAnyObjectByType<Canvas>();
        if (selectionBox != null) selectionBox.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!_dragging && EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        HandleControlGroups();

        if (Input.GetMouseButtonDown(0))
        {
            _dragStartScreen = Input.mousePosition;
            _dragging = false;
        }

        if (Input.GetMouseButton(0))
        {
            float dist = Vector2.Distance(_dragStartScreen, (Vector2)Input.mousePosition);

            if (!_dragging && dist >= clickDragThreshold)
            {
                _dragging = true;
                if (selectionBox != null)
                {
                    selectionBox.gameObject.SetActive(true);
                    UpdateSelectionBox(_dragStartScreen, Input.mousePosition);
                }
            }

            if (_dragging && selectionBox != null)
            {
                UpdateSelectionBox(_dragStartScreen, Input.mousePosition);
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (selectionBox != null)
                selectionBox.gameObject.SetActive(false);

            bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

            if (_dragging) BoxSelect(shift);
            else ClickSelect(shift);

            _dragging = false;
        }

    }

    private void ClickSelect(bool shift)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        bool hitUnit = Physics.Raycast(ray, out RaycastHit hit, 500f, unitMask);

        Unit unit = hitUnit ? hit.collider.GetComponentInParent<Unit>() : null;

        if (!shift)
            SelectionManager.Instance.ClearSelection();

        if (unit != null)
        {
            if (shift)
                SelectionManager.Instance.ToggleSelection(unit);
            else
                SelectionManager.Instance.AddToSelection(unit);
        }
        // If click empty space without shift: selection cleared already
    }

    private void BoxSelect(bool shift)
    {
        if (!shift)
            SelectionManager.Instance.ClearSelection();

        Rect rect = GetScreenRect(_dragStartScreen, Input.mousePosition);

        // Simple approach: iterate all Units in scene.
        // Later you can optimize by registering units in a list.
        Unit[] allUnits = FindObjectsByType<Unit>();
        // Preserve the InstanceID order previously returned by FindObjectsOfType.
        System.Array.Sort(allUnits, (a, b) => unchecked((int)EntityId.ToULong(a.GetEntityId())).CompareTo(unchecked((int)EntityId.ToULong(b.GetEntityId()))));
        foreach (var unit in allUnits)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(unit.transform.position);

            // ignore units behind camera
            if (screenPos.z < 0) continue;

            if (rect.Contains(screenPos))
            {
                if (shift)
                    SelectionManager.Instance.AddToSelection(unit); // shift-drag = add
                else
                    SelectionManager.Instance.AddToSelection(unit);
            }
        }
    }

    private void HandleControlGroups()
    {
        // 1..9 (and 0)
        for (int i = 0; i <= 9; i++)
        {
            KeyCode key = (i == 0) ? KeyCode.Alpha0 : KeyCode.Alpha1 + (i - 1);
            if (!Input.GetKeyDown(key)) continue;

            bool ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

            if (ctrl)
                SelectionManager.Instance.AssignGroup(i);
            else
                SelectionManager.Instance.RecallGroup(i);
        }
    }

    // --- UI Helpers ---

    private void UpdateSelectionBox(Vector2 start, Vector2 end)
    {
        if (selectionBox == null || canvas == null) return;

        Rect rect = GetScreenRect(start, end);

        // For Screen Space Overlay, anchoredPosition uses pixels.
        selectionBox.anchoredPosition = rect.position;
        selectionBox.sizeDelta = rect.size;
    }

    private static Rect GetScreenRect(Vector2 start, Vector2 end)
    {
        Vector2 bottomLeft = new Vector2(Mathf.Min(start.x, end.x), Mathf.Min(start.y, end.y));
        Vector2 topRight = new Vector2(Mathf.Max(start.x, end.x), Mathf.Max(start.y, end.y));
        return new Rect(bottomLeft, topRight - bottomLeft);
    }
}