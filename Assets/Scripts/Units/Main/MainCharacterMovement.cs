using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(NavMeshAgent))]
public class MainCharacterMovement : MonoBehaviour
{
    private NavMeshAgent _agent;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (!_agent.isActiveAndEnabled || !_agent.isOnNavMesh) return;

        // Direct control must not continue an old mouse movement order.
        if (_agent.hasPath || _agent.pathPending) _agent.ResetPath();

        if (InventoryUI.Instance != null && InventoryUI.Instance.inventoryWindow != null &&
            InventoryUI.Instance.inventoryWindow.activeInHierarchy) return;

        GameObject focused = EventSystem.current != null
            ? EventSystem.current.currentSelectedGameObject : null;
        if (focused != null && (focused.GetComponent<InputField>() != null ||
                                focused.GetComponent<TMP_InputField>() != null)) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        float horizontal = (Input.GetKey(KeyCode.D) ? 1f : 0f) - (Input.GetKey(KeyCode.A) ? 1f : 0f);
        float vertical = (Input.GetKey(KeyCode.W) ? 1f : 0f) - (Input.GetKey(KeyCode.S) ? 1f : 0f);
        Vector3 forward = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up).normalized;
        Vector3 right = Vector3.Cross(Vector3.up, forward);
        Vector3 direction = Vector3.ClampMagnitude(right * horizontal + forward * vertical, 1f);

        _agent.Move(direction * _agent.speed * Time.deltaTime);
        if (direction.sqrMagnitude > 0f)
            transform.rotation = Quaternion.RotateTowards(transform.rotation,
                Quaternion.LookRotation(direction), _agent.angularSpeed * Time.deltaTime);
    }
}
