using System.Collections.Generic;
using System;
using UnityEngine;


public enum ResourceType { Corpse, Bones, Souls, Blood }

public class Inventory : MonoBehaviour
{
    [Serializable]
    public struct Capacity
    {
        public ResourceType type;
        public int max; // <= 0 = unlimited
    }


    [Header("Starting state")]
    public InventoryLoadout loadout;   // optional
    public bool applyLoadoutOnAwake = true;

    [Header("Optional capacities (empty = unlimited)")]
    public Capacity[] capacities;

    private readonly Dictionary<ResourceType, int> _items = new();
    private readonly Dictionary<ResourceType, int> _maxByType = new();

    void Awake()
    {
        foreach (var c in capacities)
            _maxByType[c.type] = c.max;

        if (applyLoadoutOnAwake && loadout != null)
            ApplyLoadout(loadout);
    }

    public int Get(ResourceType type) => _items.TryGetValue(type, out var v) ? v : 0;

    public int SpaceLeft(ResourceType type)
    {
        if (!_maxByType.TryGetValue(type, out var max) || max <= 0) return int.MaxValue;
        return Mathf.Max(0, max - Get(type));
    }

    public int Add(ResourceType type, int amount)
    {
        if (amount <= 0) return 0;
        int add = Mathf.Min(amount, SpaceLeft(type));
        if (add <= 0) return 0;

        _items[type] = Get(type) + add;
        return add;
    }

    public int Remove(ResourceType type, int amount)
    {
        if (amount <= 0) return 0;
        int have = Get(type);
        int rem = Mathf.Min(amount, have);
        if (rem <= 0) return 0;

        int left = have - rem;
        if (left == 0) _items.Remove(type);
        else _items[type] = left;

        return rem;
    }

    public int TransferTo(Inventory other, ResourceType type, int amount)
    {
        if (other == null || amount <= 0) return 0;

        int removed = Remove(type, amount);
        int added = other.Add(type, removed);

        // refund any that didn't fit
        int refund = removed - added;
        if (refund > 0) Add(type, refund);

        return added;
    }

    public void ApplyLoadout(InventoryLoadout data, bool clearFirst = false)
    {
        if (data == null) return;

        if (clearFirst)
        {
            // clear runtime items
            // simplest: just re-create dictionary contents
            // (we’ll do a clean clear here)
            _items.Clear();
        }

        foreach (var e in data.resources)
            Add(e.type, e.amount);
    }
}