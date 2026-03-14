using UnityEngine;
using UnityEngine.Windows;

public class NewMonoBehaviourScript : MonoBehaviour
{
    // define variables for components
    //private SpriteRenderer sr; can be used for flying creatures
    private Animator animator;
    private Rigidbody2D rb;
    private Vector3 direction;
    [SerializeField] private float moveSpeed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // Set variables to components
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        //sr = GetComponent<SpriteRenderer>(); can be used for flying creatures
        animator = GetComponent<Animator>();
    }

    // Update based on physics logic for common speed on all platforms
    void FixedUpdate()
    {
        // Make the enemy face the player by flipping sprite can be used for flying creatures
      /*  if (PlayerController.PlayerInstance.transform.position.x > transform.position.x)
        {
            sr.flipX = true;
        }
        else
        {
            sr.flipX = false;
        }*/

        // move towards the player
        // if player is to right of monster, move monster to the right otherwise move left
        // if player is above the monster, move monster up otherwise move down
        // notrmalize it so directional movement is not faster then streight movement
        direction = (PlayerController.PlayerInstance.transform.position - transform.position).normalized;
        rb.linearVelocity = new Vector2 (direction.x * moveSpeed, direction.y *moveSpeed);
        // set movement direction for sprite rendering
        animator.SetFloat("moveX", rb.linearVelocityX);
        animator.SetFloat("moveY", rb.linearVelocityY);
    }
}

