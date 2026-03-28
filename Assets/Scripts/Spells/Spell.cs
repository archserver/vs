using UnityEngine;

// Base class for all spells  unused properties default to 0 for no effect
public class Spell : MonoBehaviour
{
    [Header("Identity")]
    public string displayName = "Spell";    // shown on the upgrade card so the player knows which spell is being upgraded

    [Header("Core")]
    public float castFrequency = 5f;        // seconds between each cast — used by all spells
    public float damage = 1f;               // damage dealt to enemies — used by all spells

    [Header("Area")]
    public float duration = 0f;             // how long the spell effect stays active (0 = instant)
    public float range = 0f;                // size of the affected area (0 = no area)
    public float damageFrequency = 0f;      // seconds between damage ticks (0 = one hit only)

    [Header("Pushback")]
    public float force = 0f;                // knockback force applied to enemies (0 = no pushback)

    [Header("Freeze")]
    public float freezeDuration = 0f;       // seconds enemies are frozen on hit (0 = no freeze)

    [Header("Vampiric")]
    public float vampiricPercent = 0f;      // portion of damage returned to player as health (0 = none, 0.1 = 10%)
    // for future spell plans
    [Header("Projectile")]
    public float speed = 0f;                // travel speed for projectile spells (0 = not a projectile)
    public int piercing = 0;                // number of enemies a projectile passes through before stopping

    [Header("Chain")]
    public int chainCount = 0;              // number of times a chain spell jumps between enemies
    public float chainRange = 0f;           // max distance between chain jumps
}
