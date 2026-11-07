using UnityEngine;

[CreateAssetMenu(fileName = "NewFaction", menuName = "Units/Faction")]

public class FactionDefinition : ScriptableObject
{

    [SerializeField] private string displayName;
    [SerializeField] private Color factionColor = Color.white;

    public string DisplayName => displayName;
    public Color FactionColor => factionColor;

}

