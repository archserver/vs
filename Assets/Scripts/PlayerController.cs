using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Define variable public
    public static PlayerController PlayerInstance;

    // define connectivity to object components
    private Rigidbody2D rb;
    private Animator animator;
    [SerializeField] private float moveSpeed;
    public Vector3 playerMoveDirection;

    // Check for duplicate instances for additional levels and set PlayerInstance pointer
   private void Awake()
    {
        if (PlayerInstance != null && PlayerInstance != this)
            Destroy(this);
        else
            PlayerInstance = this;
    }

    // When loading get the rigid body componet and assign it to the rb var
    // also capture the animator component
    private void Start()
    {
       rb = GetComponent<Rigidbody2D>();
       animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        // Are keys being pressed for a direction? get direction 
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputY = Input.GetAxisRaw("Vertical");
        // get directional movement normalized so diagnal does not move faster
        playerMoveDirection = new Vector2(inputX, inputY).normalized;

        // set movement direction for sprite rendering
        animator.SetFloat("moveX", inputX);
        animator.SetFloat("moveY", inputY);

        // switch movement state to on and off if there is movement
        if (inputX != 0 || inputY != 0)
        {
            animator.SetBool("ismoving", true);
        } else
        {
            animator.SetBool("ismoving", false);
        }
    }

    // movement based on physics not frame rate
    private void FixedUpdate()
    {
        // Move the direction at a fixed rate of speed
        rb.linearVelocity = new Vector2(playerMoveDirection.x * moveSpeed, playerMoveDirection.y * moveSpeed);
    }
}
