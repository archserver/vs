using Unity.Netcode;
using UnityEngine;

// Controls movement, damage, and death for ground-based enemies
public class Ground_Enemy : NetworkBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;
    private Vector3 direction;              // direction toward the player
    [SerializeField] private float moveSpeed;           // how fast the enemy moves
    [SerializeField] private float damage;              // damage dealt to the player on contact
    [SerializeField] private GameObject destructionEffect;  // effect played when enemy dies
    [SerializeField] private float health;              // how much health the enemy has
    [SerializeField] private int experienceValue;       // XP given to player on kill
    private Transform playerTransform;                 // cached player position for movement
    public float knockbackForce = 0f;                   // set by spells to slow or push back enemy
    [SerializeField] private float damageInterval = 1f; // seconds between each damage hit
    private float damageCooldown = 0f;                  // counts down to next allowed hit
    [SerializeField] private AudioSource attackSound;   // played when this creature hits the player
    [SerializeField] private AudioSource deathSound;    // played when this creature dies

    // grab required components on startup
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    // count down the damage cooldown each frame
    void Update()
    {
        if(!IsServer && !NetworkGameManager.IsSolo) return;
        
        if (damageCooldown > 0f)
            damageCooldown -= Time.deltaTime;
    }

    // physics-based movement runs on a fixed timestep for consistent speed
    void FixedUpdate()
    {
        if (!IsServer && !NetworkGameManager.IsSolo) return;

        // Find the nearest player
        playerTransform = GetNearestPlayer();

        // stop moving if player is not yet available or is dead
        if (playerTransform == null || !playerTransform.gameObject.activeSelf)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // move toward the player, subtracting any knockback force from spells
        direction = (playerTransform.position - transform.position).normalized;
        Vector2 pushBack = new Vector2(direction.x, direction.y) * knockbackForce;
        rb.linearVelocity = new Vector2(direction.x * moveSpeed, direction.y * moveSpeed) - pushBack;

        // update animator so the correct walk direction sprite plays
        animator.SetFloat("moveX", rb.linearVelocityX);
        animator.SetFloat("moveY", rb.linearVelocityY);
    }

    // deal damage to the player once per damageInterval while in contact
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!IsServer && !NetworkGameManager.IsSolo) return;
        if (collision.gameObject.CompareTag("Player") && damageCooldown <= 0f)
        {
            if (attackSound != null) AudioController.ACInstance.PlaySound(attackSound);
            // damage whichever player was hit, not just the local player
            PlayerController pc = collision.gameObject.GetComponent<PlayerController>();
            if (pc != null) 
                pc.DamagePlayer(damage);
            damageCooldown = damageInterval;
        }
    }

    // called by spells — reduce health, show damage number, return vampiric health, destroy if dead
    public void TakeDamage(float damageTaken, float vampiricPercent = 0f)
    {
        if (!IsServer && !NetworkGameManager.IsSolo) return;
        health -= damageTaken;
        GameStats.GSInstance.totalDamageDone += damageTaken;
        DamageNumberController.DNCInstance.CreateNumber(damageTaken, transform.position);

        // if the spell has a vampiric effect, return a portion of the damage as health to the player
        if (vampiricPercent > 0f)
            PlayerController.PlayerInstance.HealPlayer(damageTaken * vampiricPercent);

        if (health <= 0)
        {
            // play death effect, give player XP, remove enemy from scene
            
            PlayerController.PlayerInstance.GainExperience(experienceValue);
            GameStats.GSInstance.zombiesKilled++;
            if (deathSound != null) AudioController.ACInstance.PlayModifiedSound(deathSound);

            if (NetworkGameManager.IsSolo)
            {
                Instantiate(destructionEffect, transform.position, transform.rotation);
                Destroy(gameObject);
            }
            else
            {
                SpawnDeathEffectRpc(transform.position, transform.rotation);
                GetComponent<NetworkObject>().Despawn(true);
            }
        }
    }

    // spawn the visual death effect on all clients
    [Rpc(SendTo.ClientsAndHost)]
    private void SpawnDeathEffectRpc(Vector3 position, Quaternion rotation)
    {
        Instantiate(destructionEffect, position, rotation);
    }

    // returns the nearest player transform — for both solo and multiplayer
    private Transform GetNearestPlayer()
    {
         if (NetworkGameManager.IsSolo)
              return PlayerController.PlayerInstance?.transform;

         // get list of players in the game
        var players = PlayerController.AllPlayers;
         Transform nearest = null;
         float minDist = float.MaxValue;
         foreach (var p in players)
         {
            if (!p.gameObject.activeSelf) continue;
            float dist = Vector3.Distance(transform.position, p.transform.position);
            if (dist < minDist) 
            { 
                minDist = dist; nearest = p.transform;
            }
         }
         return nearest;
    }
}
