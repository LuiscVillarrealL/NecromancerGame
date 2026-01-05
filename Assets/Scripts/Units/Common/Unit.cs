using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField] private GameObject selectionVisual;

    public UnitMovement Movement { get; private set; }

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