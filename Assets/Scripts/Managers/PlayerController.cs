using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

// Handles player movement, health, experience, and leveling
// With multiplayer server is health and xp authoritative
public class PlayerController : NetworkBehaviour
{
    public static PlayerController PlayerInstance;              // pointer for local player in solo or multiplayer
    public static List<PlayerController> AllPlayers = new();   // all active players — used by enemies to find nearest target

    private Rigidbody2D rb;                         // physics body for movement
    private Animator animator;                      // blend tree animator for directional walking sprites
    [SerializeField] private float moveSpeed;       // how fast the player moves
    public Vector3 playerMoveDirection;             // current normalized movement direction
    public float playerMaxHealth;                   // maximum health set in Inspector
    // For solo play
    public float playerHealth;                      // current health
    public float playerExperience;                  // current XP toward next level
    public int playerLevel = 1;                     // current player level
    public int xpThreshold = 2;                     // XP needed to reach next level, grows each level up
    // For network play
    private NetworkVariable<float> netHealth = new NetworkVariable<float>(writePerm: NetworkVariableWritePermission.Server);
    private NetworkVariable<float> netExperience = new NetworkVariable<float>(writePerm: NetworkVariableWritePermission.Server);
    private NetworkVariable<int> netLevel = new NetworkVariable<int>(writePerm: NetworkVariableWritePermission.Server);
    private NetworkVariable<int> netXPThreshold = new NetworkVariable<int>(writePerm: NetworkVariableWritePermission.Server);

    // block local movement input when leveling up server still runs

    private bool levelingUp = false;

    public override void OnNetworkSpawn()
    {
        AllPlayers.Add(this);
        // Playing this is a local copy of the player
        if (IsOwner || NetworkGameManager.IsSolo)
        {
            PlayerInstance = this;
            StartCoroutine(AssignCamera());

            // set server settings if multiiplayer
            if (!NetworkGameManager.IsSolo)
            {
                netHealth.OnValueChanged += (_, v) => { playerHealth = v; UIController.UIInstance?.UpdateHealthSlider(); };
                netExperience.OnValueChanged += (_, v) => { playerExperience = v; UIController.UIInstance?.UpdateExpSlider(); };
                netLevel.OnValueChanged += (_, v) => { playerLevel = v; UIController.UIInstance?.UpdateExpSlider(); };
                netXPThreshold.OnValueChanged += (_, v) => { xpThreshold = v; UIController.UIInstance?.UpdateExpSlider(); };
            }
        }
    }

    // initialize health and UI on scene start
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        if (NetworkGameManager.IsSolo) AllPlayers.Add(this);
        if (!IsOwner && !NetworkGameManager.IsSolo) return;
        if (NetworkGameManager.IsSolo) StartCoroutine(AssignCamera());

        playerHealth = playerMaxHealth;

        // setup network attributes — use playerMaxHealth directly so server sets correct value for all players
        if (!NetworkGameManager.IsSolo && IsServer)
        {
                netHealth.Value = playerMaxHealth;
                netXPThreshold.Value = xpThreshold;
                netLevel.Value = playerLevel;
        }
        UIController.UIInstance?.UpdateHealthSlider();
        UIController.UIInstance?.UpdateExpSlider();
        GameManager.GMInstance.gameIsActive = true;
    }

    // read input and move player 
    void Update()
    {
        // return if not the owner
        if (!IsOwner && !NetworkGameManager.IsSolo) return;
        // return if in level up
        if (levelingUp) return;

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

        // read gamepad stick if a controller is connected
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
        // if not player for device return
        if(!IsOwner && !NetworkGameManager.IsSolo) return;
        rb.linearVelocity = new Vector2(playerMoveDirection.x * moveSpeed, playerMoveDirection.y * moveSpeed);
    }

    // reduce health and trigger game over if health reaches zero
    public void DamagePlayer(float damage)
    {
        if (NetworkGameManager.IsSolo)
            ApplyDamage(damage);
        else
            DamagePlayerRpc(damage);
    }

    // server deals damage to the players
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void DamagePlayerRpc(float damage)
    {
        ApplyDamage(damage);
    }

    private void ApplyDamage(float damage) {
        playerHealth -= damage;
        if (GameStats.GSInstance != null) GameStats.GSInstance.totalDamageTaken += damage;
        if (!NetworkGameManager.IsSolo)
            netHealth.Value = playerHealth;
        UIController.UIInstance?.UpdateHealthSlider();

        if (playerHealth <= 0)
        {
            gameObject.SetActive(false);
            if (!NetworkGameManager.IsSolo) DisablePlayerRpc();
            // only trigger game over when no players remain alive
            bool anyAlive = false;
            foreach (var p in AllPlayers)
            {
                if (p != this && p.gameObject.activeSelf && p.playerHealth > 0)
                {
                    anyAlive = true;
                    break;
                }
            }
            if (!anyAlive)
                GameManager.GMInstance.GameOver();
        }
    }

    // restore health from a heart pickup, capped at max health
    public void HealPlayer(float amount)
    {
        if (NetworkGameManager.IsSolo)
            ApplyHealth(amount);
        else
            HealPlayerRpc(amount);
    }

    // Server heals the player
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void HealPlayerRpc(float amount) 
    {
            ApplyHealth(amount);
    }

    // apply the health to the player
    private void ApplyHealth(float amount) 
    {
        playerHealth = Mathf.Min(playerHealth + amount, playerMaxHealth);
        GameStats.GSInstance.totalHealthRecovered += amount;
        if(!NetworkGameManager.IsSolo) 
            netHealth.Value = playerHealth;

        UIController.UIInstance.UpdateHealthSlider();
    }

    // add XP from a killed enemy and check if the player levels up
    public void GainExperience(int experience)
    {
        if (NetworkGameManager.IsSolo)
            ApplyExperience(experience);
        else
            GainExperienceRpc(experience);
    }

    //
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void GainExperienceRpc(int experience)
    {
        ApplyExperience(experience);
    }

    // apply the experience to the player
    private void ApplyExperience (int experience) {
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
            if (!NetworkGameManager.IsSolo)
            {
                netExperience.Value = playerExperience;
                netLevel.Value = playerLevel;
                netXPThreshold.Value = xpThreshold;
            }
            if (NetworkGameManager.IsSolo)
                LevelUpManager.LUMInstance.ShowLevelUp();
            else
                ShowLevelUpRpc();                                    // send to owning client's device
        }
        if (!NetworkGameManager.IsSolo)
            netExperience.Value = playerExperience;
        UIController.UIInstance.UpdateExpSlider();
    }

    // hides the player on their own device when they die
    [Rpc(SendTo.Owner)]
    private void DisablePlayerRpc()
    {
        gameObject.SetActive(false);
    }

    // tells the owning client to show the level up screen on their device
    [Rpc(SendTo.Owner)]
    private void ShowLevelUpRpc()
    {
        LevelUpManager.LUMInstance.ShowLevelUp();
    }

    private IEnumerator AssignCamera()
    {
        yield return null; // wait one frame for scene objects to be ready
        var camObj = GameObject.Find("CinemachineCamera");
        Debug.Log($"[PlayerController] AssignCamera — camObj found: {camObj != null}, owner: {IsOwner}, solo: {NetworkGameManager.IsSolo}");
        if (camObj != null)
        {
            var vcam = camObj.GetComponent<CinemachineCamera>();
            if (vcam != null)
            {
                vcam.Target.TrackingTarget = transform;
                Debug.Log($"[PlayerController] Tracking target set to {gameObject.name}");
            }
        }
    }

    public override void OnDestroy()
    {
        AllPlayers.Remove(this);
        base.OnDestroy();
    }

    // instead of timescale pause
    public void SetLevelingUp(bool value)
    {
        levelingUp = value;
    }
}
