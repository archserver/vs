using UnityEngine;

public class FlyingEnemy : MonoBehaviour
{
    // define variables for components
    //field for speed
    // field for destruction Animation
    private SpriteRenderer sr; //can be used for flying creatures
    private Animator animator;
    private Rigidbody2D rb;
    private Vector3 direction;
    [SerializeField] private float moveSpeed;
    [SerializeField] private GameObject destructionEffect;
    private Transform _playerTransform;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // Set variables to components
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>(); //can be used for flying creatures
        animator = GetComponent<Animator>();
        _playerTransform = PlayerController.PlayerInstance.transform;
    }

    // Update baseed on physics logic
    void FixedUpdate()
    {
        if (PlayerController.PlayerInstance == null) return;
        // Make the enemy face the player by flipping sprite can be used for flying creatures
        if (PlayerController.PlayerInstance.transform.position.x > transform.position.x)
          {
              sr.flipX = true;
          }
          else
          {
              sr.flipX = false;
          }

        // move towards the player
        // if player is to right of monster, move monster to the right otherwise move left
        // if player is above the monster, move monster up otherwise move down
        // notrmalize it so directional movement is not faster then streight movement
        direction = (_playerTransform.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(direction.x * moveSpeed, direction.y * moveSpeed);
        // set movement direction for sprite rendering
        animator.SetFloat("moveX", rb.linearVelocityX);
        animator.SetFloat("moveY", rb.linearVelocityY);
    }

    // create destruction effect and destroy the crreature
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Instantiate(destructionEffect, transform.position, transform.rotation);
            Destroy(gameObject);
        }
    }
}
