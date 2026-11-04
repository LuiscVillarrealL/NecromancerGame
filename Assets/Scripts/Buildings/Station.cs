using UnityEngine;

[RequireComponent(typeof(Inventory))]
public class Station : MonoBehaviour
{
    public string stationName = "Station";
    public float interactRange = 2.5f;

    public Inventory Inventory { get; private set; }

    void Awake()
    {
        Inventory = GetComponent<Inventory>();
    }
}
