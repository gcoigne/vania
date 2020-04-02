using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pixel_Movement : MonoBehaviour
{
    #region References to objects
    private Animator anim;                      // Reference to the player's animator component.
    private Rigidbody2D rb;                     // Reference to the player's physics object.
    private BoxCollider2D hitbox;               // The player's main hitbox.
    private BoxCollider2D groundCheck;          // A hitbox that checks if the player is grounded.
    private BoxCollider2D groundLeft;
    private BoxCollider2D groundRight;
    private LayerMask groundMask;               // Reference to Ground layer mask.
    public GameObject playerAttack;             // Reference to the attack prefab.
    #endregion

    #region Input variables
    private float hInput;                       // Horizontal input.
    private float vInput;                       // Vertical input.

    private bool jumpPress = false;             // Whether or not the player is pressing jump.
    private bool jumpHold = false;              // Whether or not the player is holding jump.

    private bool dashPress = false;             // Whether or not the player is pressing dash.
    private bool dashHold = false;              // Whether or not the player is holding dash.

    private bool attackPress = false;           // Whether or not the player is pressing attack.
    private bool attackHold = false;            // Whether or not the player is holding attack.

    private bool interactPress = false;           // Whether or not the player is pressing interact.
    private bool interactHold = false;            // Whether or not the player is holding interact.
    #endregion

    #region Physics variables
    public bool grounded = false;
    public float gravScale = 1;
    public bool facingRight = true;
    #endregion

    #region State Variables
    private string state = "standing";          // Current physics state.
    private float stateStart = 0f;              // Time (frames) when the player switched into the current state.
    private string action = "none";             // Player's current action.
    private float actionStart = 0f;             // Time (frames) when the player started current action.
    private float actionDur = 0f;               // Duration of current action.

    // Standing State
    public float moveForce = 256f;              // Amount of force added to move the player left and right.
    public float airForce = 64f;                // Amount of horizontal force added while midair.
    public float maxSpeed = 5f;                 // The fastest the player can travel in the x axis.

    [HideInInspector]
    public bool standing = false;               // Whether or not the player is standing.

    // Falling State
    [HideInInspector]
    public bool falling = false;                // Whether or not the player is falling.

    // Terminal Falling State
    public float terminalFallSpeed = 50;        // The player's terminal velocity (triggers terminalFall when reached).
    [HideInInspector]
    public bool terminalFalling = false;        // Whether or not the player is terminalFalling.

    // Jumping State
    public int airJumps = 1;                    // Numer of times the player can jump in midair before landing.
    public float jumpForceInitial = 60f;        // Amount of force added when the player jumps.
    public float jumpForceAdded = 1f;           // Amount of force added each frame while the player holds jump.
    public float jumpDur = 30f;                 // Maximum amount of time (frames) a player can gain jump speed.
    public int jumps = 0;                       // Number of air jumps the player has available.
    [HideInInspector]
    public bool jumping = false;                // Whether or not the player is jumping.

    // Crouching State
    [HideInInspector]
    public bool crouching = false;              // Whether or not the player is crouching.

    // Dashing State
    public int maxDashes = 1;                   // Number of times the player can dash before landing/waiting.
    public float dashSpeed = 20f;               // Speed the player travels while dashing.
    public float dashDur = 50f;                // Time(ms) a dash lasts.
    public float dashCooldown = 2000f;          // Time(ms) dashes take to cooldown.
    [HideInInspector]
    public int dashes = 0;                      // Number of dashes the player has available.
    [HideInInspector]
    public bool dashing = false;                // Whether or not the player is dashing.

    // Attacking State
    [HideInInspector]
    public bool canAttack = true;               // Whether or not the player can attack.
    [HideInInspector]
    public bool attacking = false;              // Whether or not the player is attacking.
    #endregion

    // Use this for initialization
    void Start()
    {
        #region Set up references.
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        hitbox = GetComponent<BoxCollider2D>();
        Transform groundTriggers = transform.Find("GroundTriggers");
        groundCheck = groundTriggers.Find("Center").GetComponent<BoxCollider2D>();
        groundLeft = groundTriggers.Find("LeftWhisker").GetComponent<BoxCollider2D>();
        groundRight = groundTriggers.Find("RightWhisker").GetComponent<BoxCollider2D>();
        groundMask = LayerMask.GetMask("Ground");
        #endregion
    }

    // Update is called once per frame. It is mainly used for caching user input and rendering.
    void Update()
    {
        #region Cache the user's input.
        hInput = Input.GetAxis("Horizontal");
        vInput = Input.GetAxis("Vertical");
        jumpPress = Input.GetButtonDown("Jump");
        jumpHold = Input.GetButton("Jump");
        //dashPress = Input.GetButtonDown("Dash");
        //dashHold = Input.GetButton("Dash");
        attackPress = Input.GetButtonDown("Fire1");
        attackHold = Input.GetButton("Fire1");
        interactPress = Input.GetButtonDown("Interact");
        interactHold = Input.GetButtonDown("Interact");
        #endregion
    }

    // FixedUpdate runs once each physics engine frame. It is mainly used for applying forces and other physics-related events.
    void FixedUpdate()
    {
        #region Physics variables
        // The player is grounded if a linecast to the groundcheck position hits anything on the ground layer.
        grounded = Physics2D.IsTouchingLayers(groundCheck, groundMask);
        if (grounded)
        {
            jumps = airJumps;
        }
        #endregion

        #region Movement
        Vector2 vel = rb.velocity;
        
        // If the player is in motion but has no horizontal input, stop them.
        if (hInput == 0 && rb.velocity.x != 0)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }

        // If the player's horizontal velocity is less than max speed, set their speed to max
        else if (Mathf.Sign(hInput) * vel.x < maxSpeed)
            if (grounded)
            {
                rb.velocity = new Vector2(maxSpeed * Mathf.Sign(hInput), rb.velocity.y);
            }
            else
            {
                rb.velocity = new Vector2(maxSpeed, rb.velocity.y);
            }

        // If the player's horizontal velocity is greater than the maxSpeed set it equal to the maxSpeed.
        if (Mathf.Abs(vel.x) > maxSpeed)
            rb.velocity = new Vector2(maxSpeed * Mathf.Sign(hInput), rb.velocity.y);

        #endregion
    }

    // Turns the player around.
    private void turnAround()
    {
        // Switch the way the player is labelled as facing.
        facingRight = !facingRight;

        // Multiply the player's x local scale by -1.
        //Vector3 theScale = playerBody.localScale;
        //theScale.x *= -1;
        //playerBody.localScale = theScale;
    }
}
