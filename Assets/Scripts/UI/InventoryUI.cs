using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance { get; private set; }

    [Header("Window")]
    public GameObject inventoryWindow;
    public TMP_Text titleText; // or TMP_Text if you use TMP
    public Button closeButton;

    [Header("Lists")]
    public Transform leftContent;   // player
    public Transform rightContent;  // station

    [Header("Prefab")]
    public InventoryRowUI rowPrefab;

    private Inventory _playerInv;
    private Inventory _stationInv;
    private readonly List<GameObject> _spawned = new();

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);

        if (inventoryWindow != null)
            inventoryWindow.SetActive(false);
    }

    public void Open(Inventory playerInv, Inventory stationInv, string stationName)
    {
        _playerInv = playerInv;
        _stationInv = stationInv;

        if (titleText != null) titleText.text = stationName;
        if (inventoryWindow != null) inventoryWindow.SetActive(true);

        Rebuild();
    }

    public void Close()
    {
        if (inventoryWindow != null) inventoryWindow.SetActive(false);
        _playerInv = null;
        _stationInv = null;
        ClearRows();
    }

    void Update()
    {
        if (inventoryWindow != null && inventoryWindow.activeSelf && Input.GetKeyDown(KeyCode.Escape))
            Close();
    }

    public void Rebuild()
    {
        ClearRows();
        if (_playerInv == null || _stationInv == null) return;

        foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
        {
            // Player row (moves to station)
            SpawnRow(leftContent, type,
                getCount: () => _playerInv.Get(type),
                move1: () => { _playerInv.TransferTo(_stationInv, type, 1); Rebuild(); },
                move5: () => { _playerInv.TransferTo(_stationInv, type, 5); Rebuild(); },
                moveAll: () => { _playerInv.TransferTo(_stationInv, type, _playerInv.Get(type)); Rebuild(); }
            );

            // Station row (moves to player)
            SpawnRow(rightContent, type,
                getCount: () => _stationInv.Get(type),
                move1: () => { _stationInv.TransferTo(_playerInv, type, 1); Rebuild(); },
                move5: () => { _stationInv.TransferTo(_playerInv, type, 5); Rebuild(); },
                moveAll: () => { _stationInv.TransferTo(_playerInv, type, _stationInv.Get(type)); Rebuild(); }
            );
        }
    }

    private void SpawnRow(Transform parent, ResourceType type,
        Func<int> getCount, Action move1, Action move5, Action moveAll)
    {
        var row = Instantiate(rowPrefab, parent);
        row.Bind(type, getCount, move1, move5, moveAll);
        _spawned.Add(row.gameObject);
    }

    private void ClearRows()
    {
        foreach (var go in _spawned)
            if (go) Destroy(go);
        _spawned.Clear();
    }
}
