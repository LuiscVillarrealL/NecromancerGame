using System;
using UnityEngine;


[CreateAssetMenu(fileName = "NewFactionRelation", menuName = "Units/FactionRelation")]
public class FactionRelations : ScriptableObject
{

    public enum FactionRelationship
{
    Neutral,
    Allied,
    Hostile
}

[Serializable]
    public struct FactionRelation
    {
        public FactionDefinition factionA;
        public FactionDefinition factionB;
        public FactionRelationship relationship;
    }

[SerializeField]
    private FactionRelation[] relations = Array.Empty<FactionRelation>();

    public FactionRelationship GetRelationship(
        FactionDefinition a,
        FactionDefinition b)
    {
        if (a == null || b == null)
            return FactionRelationship.Neutral;

        if (a == b)
            return FactionRelationship.Allied;

        foreach (FactionRelation entry in relations)
        {
            bool matches =
                (entry.factionA == a && entry.factionB == b) ||
                (entry.factionA == b && entry.factionB == a);

            if (matches)
                return entry.relationship;
        }

        return FactionRelationship.Neutral;
    }

}
