using UnityEngine;

// Point Blank Area of Effect spell — spawns a growing area around the player that damages and pushes back enemies
// Inherits all spell properties from Spell base class
public class PBAOE : Spell
{
    [SerializeField] private GameObject prefab;     // the visual effect prefab to spawn each cast

    private float spawnCounter;                     // counts down to the next cast

    void Update()
    {
        // count down and spawn a new spell effect when the cast frequency timer expires
        spawnCounter -= Time.deltaTime;
        if (spawnCounter <= 0)
        {
            spawnCounter = castFrequency;
            Instantiate(prefab, transform.position, transform.rotation, transform);
            GameStats.GSInstance.spellsCast++;
        }
    }
}
