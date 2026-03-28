using UnityEngine;

// Defines the possible spell properties that can be upgraded
public enum UpgradeProperty
{
    Damage,
    CastFrequency,
    Duration,
    Range,
    DamageFrequency,
    Force,
    FreezeDuration,
    VampiricPercent,
    Speed,
    Piercing,
    ChainCount,
    ChainRange
}

// A single upgrade card definition — used to create asset in the Unity Editor
[CreateAssetMenu(fileName = "SpellUpgrade", menuName = "Spell Upgrade")]
public class SpellUpgrade : ScriptableObject
{
    public string upgradeName;          // short name shown on the card e.g. "More Damage"
    public string description;          // detail shown on the card e.g. "Increases damage by 1"
    public Sprite icon;                 // icon shown on the card
    public UpgradeProperty property;    // which spell property this upgrade affects
    public float upgradeAmount;         // how much to add to the property
}
