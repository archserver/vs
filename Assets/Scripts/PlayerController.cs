using UnityEngine;
using UnityEngine.InputSystem;

// Handles player movement, health, experience, and leveling
public class PlayerController : MonoBehaviour
{
    public static PlayerController PlayerInstance;  // pointer so any script can access the player

    private Rigidbody2D rb;                         // physics body for movement
    private Animator animator;                      // blend tree animator for directional walking sprites
    [SerializeField] private float moveSpeed;       // how fast the player moves
    public Vector3 playerMoveDirection;             // current normalized movement direction
    public float playerMaxHealth;                   // maximum health set in Inspector
    public float playerHealth;                      // current health
    public float playerExperience;                  // current XP toward next level
    public int playerLevel = 1;                     // current player level
    public int xpThreshold = 2;                     // XP needed to reach next level, grows each level up

    // prevent duplicate player instances when changing scenes
    private void Awake()
    {
        if (PlayerInstance != null && PlayerInstance != this)
            Destroy(this);
        else
            PlayerInstance = this;
    }

    // initialize health and UI on scene start
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerHealth = playerMaxHealth;
        UIController.UIInstance.UpdateHealthSlider();
        UIController.UIInstance.UpdateExpSlider();
        GameManager.GMInstance.gameIsActive = true;
    }

    // read input and update animator every frame
    void Update()
    {
        // read keyboard input
        float inputX = 0f;
        float inputY = 0f;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed) inputX = -1f;
            if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed) inputX = 1f;
            if (Keyboard.current.downArrowKey.isPressed || Keyboard.current.sKey.isPressed) inputY = -1f;
            if (Keyboard.current.upArrowKey.isPressed || Keyboard.current.wKey.isPressed) inputY = 1f;
        }

        // read gamepad left stick if a controller is connected
        if (Gamepad.current != null)
        {
            Vector2 stick = Gamepad.current.leftStick.ReadValue();
            if (stick.sqrMagnitude > 0.01f)
            {
                inputX = stick.x;
                inputY = stick.y;
            }
        }

        // if joystick is active and being used, let it override keyboard
        if (VirtualJoystick.JoystickInstance != null && VirtualJoystick.JoystickInstance.gameObject.activeInHierarchy)
        {
            Vector2 joystick = VirtualJoystick.JoystickInstance.InputDirection;
            if (joystick.sqrMagnitude > 0f)
            {
                inputX = joystick.x;
                inputY = joystick.y;
            }
        }

        // normalize so diagonal movement isn't faster than straight movement
        playerMoveDirection = new Vector2(inputX, inputY).normalized;

        // tell animator which direction the player is facing
        animator.SetFloat("moveX", inputX);
        animator.SetFloat("moveY", inputY);

        // toggle walk animation on or off
        animator.SetBool("ismoving", inputX != 0 || inputY != 0);
    }

    // apply movement using physics so speed is consistent regardless of frame rate
    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(playerMoveDirection.x * moveSpeed, playerMoveDirection.y * moveSpeed);
    }

    // reduce health and trigger game over if health reaches zero
    public void DamagePlayer(float damage)
    {
        playerHealth -= damage;
        GameStats.GSInstance.totalDamageTaken += damage;
        UIController.UIInstance.UpdateHealthSlider();
        if (playerHealth <= 0)
        {
            gameObject.SetActive(false);
            GameManager.GMInstance.GameOver();
        }
    }

    // restore health from a heart pickup, capped at max health
    public void HealPlayer(float amount)
    {
        playerHealth = Mathf.Min(playerHealth + amount, playerMaxHealth);
        GameStats.GSInstance.totalHealthRecovered += amount;
        UIController.UIInstance.UpdateHealthSlider();
    }

    // add XP from a killed enemy and check if the player levels up
    public void GainExperience(int experience)
    {
        playerExperience += experience;
        GameStats.GSInstance.totalXPGained += experience;

        // level up if XP has reached the threshold
        if (playerExperience >= xpThreshold)
        {
            playerExperience -= xpThreshold;                        // carry over any excess XP
            playerLevel++;
            xpThreshold = (int)((xpThreshold + 2) * 1.15f);        // next level requires more XP
            if (playerLevel > GameStats.GSInstance.highestLevel)
                GameStats.GSInstance.highestLevel = playerLevel;
            LevelUpManager.LUMInstance.ShowLevelUp();               // show upgrade card selection
        }
        UIController.UIInstance.UpdateExpSlider();
    }
}
