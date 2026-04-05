using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

// Controls the visual effect and behavior of the PBAOE spell instance
public class PBAOEPrefab : NetworkBehaviour
{
    public PBAOE spell;                                     // reference to spell settings
    private Vector2 endSize;                                // target size the effect grows to
    private float countdownTimer;                           // tracks how long the spell has left until spawn
    public List<Ground_Enemy> groundEnemiesInRange;         // ground enemies currently inside the spell
    public List<FlyingEnemy> flyingEnemiesInRange;          // flying enemies currently inside the spell
    private float damageFrequencyCountdown;                 // timer between damage ticks
    [SerializeField] private AudioSource spellsound;        // audio file the play when cast
    private Transform _ownerTransform;                      // the player this spell belongs to

    // get spell settings and set starting size to zero so it grows
    void Start()
    {
        spell = GameObject.Find("PBAOE").GetComponent<PBAOE>();
        endSize = Vector2.one * spell.range;    // grow to the spell's range
        transform.localScale = Vector2.zero;    // start at 0 size
        countdownTimer = spell.duration;        // set the duration
        if (spellsound != null) spellsound.Play();

        // cache the owning player's transform so we can follow them in multiplayer
        if (!NetworkGameManager.IsSolo)
            _ownerTransform = PlayerController.PlayerInstance?.transform;
    }

    // stop the sound when the spell effect is destroyed
    public override void OnDestroy()
    {
        if (spellsound != null) spellsound.Stop();
    }

    void Update()
    {
        // in multiplayer follow the owning player on the server
        if (!NetworkGameManager.IsSolo && IsServer && _ownerTransform != null)
            transform.position = _ownerTransform.position;

        // grow the spell effect outward until it reaches full size
        transform.localScale = Vector2.MoveTowards(transform.localScale, endSize, Time.deltaTime * 3);

        // count down the spell duration
        countdownTimer -= Time.deltaTime;
        if (countdownTimer <= 0)
        {
            // once expired, shrink back to zero then destroy
            endSize = Vector2.zero;
            if (transform.localScale.x <= 0f)
            {
                if (NetworkGameManager.IsSolo)
                    Destroy(gameObject);
                else
                    GetComponent<NetworkObject>().Despawn(true);
            }
        }

        // damage ticks only run on the server
        if (!IsServer && !NetworkGameManager.IsSolo) return;

        damageFrequencyCountdown -= Time.deltaTime;
        if (damageFrequencyCountdown <= 0f)
        {
            damageFrequencyCountdown = spell.damageFrequency;
            for (int i = 0; i < groundEnemiesInRange.Count; i++)
                groundEnemiesInRange[i].TakeDamage(spell.damage, spell.vampiricPercent);
            for (int j = 0; j < flyingEnemiesInRange.Count; j++)
                flyingEnemiesInRange[j].TakeDamage(spell.damage, spell.vampiricPercent);
        }
    }

    // when an enemy enters the spell area, add to tracking list and apply knockback
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (!IsServer && !NetworkGameManager.IsSolo) return;
        if (collider.CompareTag("Enemy"))
        {
            Ground_Enemy groundEnemy = collider.gameObject.GetComponent<Ground_Enemy>();
            if (groundEnemy == null)
            {
                // must be a flying enemy
                FlyingEnemy flyingEnemy = collider.GetComponent<FlyingEnemy>();
                flyingEnemiesInRange.Add(flyingEnemy);
                flyingEnemy.knockbackForce = spell.force;
            }
            else
            {
                groundEnemiesInRange.Add(groundEnemy);
                groundEnemy.knockbackForce = spell.force;
            }
        }
    }

    // when an enemy leaves the spell area, remove from tracking list and clear knockback
    private void OnTriggerExit2D(Collider2D collider)
    {
        if (!IsServer && !NetworkGameManager.IsSolo) return;
        if (collider.CompareTag("Enemy"))
        {
            Ground_Enemy groundEnemy = collider.gameObject.GetComponent<Ground_Enemy>();
            if (groundEnemy == null)
            {
                // must be a flying enemy
                FlyingEnemy flyingEnemy = collider.GetComponent<FlyingEnemy>();
                flyingEnemiesInRange.Remove(flyingEnemy);
                flyingEnemy.knockbackForce = 0f;
            }
            else
            {
                groundEnemiesInRange.Remove(groundEnemy);
                groundEnemy.knockbackForce = 0f;
            }
        }
    }
}
