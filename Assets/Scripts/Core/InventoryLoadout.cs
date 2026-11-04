using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewInventoryLoadout", menuName = "Inventory/Inventory Loadout")]
public class InventoryLoadout : ScriptableObject
{
    [Serializable]
    public struct Resource
    {
        public ResourceType type;
        public int amount;
    }

    public Resource[] resources;
}
