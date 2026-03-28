using UnityEngine;

// Controls movement, damage, and death for flying enemies
public class FlyingEnemy : MonoBehaviour
{
    private SpriteRenderer sr;              // used to flip sprite to face the player
    private Animator animator;
    private Rigidbody2D rb;
    private Vector3 direction;              // direction toward the player
    [SerializeField] private float moveSpeed;           // how fast the enemy moves
    [SerializeField] private float damage;              // damage dealt to the player on contact
    [SerializeField] private GameObject destructionEffect;  // effect played when enemy dies
    [SerializeField] private float health;              // how much health the enemy has
    [SerializeField] private int experienceValue;       // XP given to player on kill
    private Transform _playerTransform;                 // cached player position for movement
    public float knockbackForce = 0f;                   // set by spells to slow or push back enemy
    [SerializeField] private float damageInterval = 1f; // seconds between each damage hit
    private float damageCooldown = 0f;                  // counts down to next allowed hit
    [SerializeField] private AudioSource attackSound;   // played when this creature hits the player
    [SerializeField] private AudioSource deathSound;    // played when this creature dies

    // grab required components on startup
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        _playerTransform = PlayerController.PlayerInstance.transform;
    }

    // count down the damage cooldown each frame
    void Update()
    {
        if (damageCooldown > 0f)
            damageCooldown -= Time.deltaTime;
    }

    // physics-based movement runs on a fixed timestep for consistent speed
    void FixedUpdate()
    {
        // wait for player to be ready before moving
        if (_playerTransform == null)
        {
            if (PlayerController.PlayerInstance != null)
                _playerTransform = PlayerController.PlayerInstance.transform;
        }

        // stop moving if player is not yet available or is dead
        if (_playerTransform == null || !PlayerController.PlayerInstance.gameObject.activeSelf)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // flip sprite to face the player based on horizontal position
        sr.flipX = _playerTransform.position.x > transform.position.x;

        // move toward the player, subtracting any knockback force from spells
        direction = (_playerTransform.position - transform.position).normalized;
        Vector2 pushBack = new Vector2(direction.x, direction.y) * knockbackForce;
        rb.linearVelocity = new Vector2(direction.x * moveSpeed, direction.y * moveSpeed) - pushBack;

        // update animator so the correct walk direction sprite plays
        animator.SetFloat("moveX", rb.linearVelocityX);
        animator.SetFloat("moveY", rb.linearVelocityY);
    }

    // deal damage to the player once per damageInterval while in contact
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && damageCooldown <= 0f)
        {
            if (attackSound != null) AudioController.ACInstance.PlaySound(attackSound);
            PlayerController.PlayerInstance.DamagePlayer(damage);
            damageCooldown = damageInterval;
        }
    }

    // called by spells — reduce health, show damage number, return vampiric health, destroy if dead
    public void TakeDamage(float damageTaken, float vampiricPercent = 0f)
    {
        health -= damageTaken;
        GameStats.GSInstance.totalDamageDone += damageTaken;
        DamageNumberController.DNCInstance.CreateNumber(damageTaken, transform.position);

        // if the spell has a vampiric effect, return a portion of the damage as health to the player
        if (vampiricPercent > 0f)
            PlayerController.PlayerInstance.HealPlayer(damageTaken * vampiricPercent);

        if (health <= 0)
        {
            // play death effect, give player XP, remove enemy from scene
            Instantiate(destructionEffect, transform.position, transform.rotation);
            PlayerController.PlayerInstance.GainExperience(experienceValue);
            GameStats.GSInstance.wyvernsKilled++;
            if (deathSound != null) AudioController.ACInstance.PlayModifiedSound(deathSound);
            Destroy(gameObject);
        }
    }
}
