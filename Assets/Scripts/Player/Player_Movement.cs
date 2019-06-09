using UnityEngine;
using System.IO;

public class Player_Movement : MonoBehaviour
{
    #region References to objects
    private Rigidbody2D rb;                     // Reference to the player's physics object.
    private BoxCollider2D hitbox;               // The player's main hitbox.
    private BoxCollider2D groundCheck;          // A hitbox that checks if the player is grounded.
    private BoxCollider2D groundLeft;
    private BoxCollider2D groundRight;
    private Animator anim;                      // Reference to the player's animator component.
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
    #endregion

    #region Physics variables
    public bool grounded = false;               // Whether or not the player is in contact with the ground.
    public float gravScale = 1;
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
    public bool facingRight = true;             // For determining which way the player is currently facing.
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

    #region Animation cancels

    //// Whether or not a player may cancel from the first state into the second via input.
    //// Some cancels lead to unique moves.
    //// This section seems like baggage, as you'll need to develop custom actions individually. Its only real use is enabling cancels when the player
    //// gains an ability, but it should be done on ability basis, not input basis. The current layout is long and clogs Inspector w/ public fields.

    //// into jumping
    //public bool jumpJumpCancel = true;          // Air jump before the first jump ends.
    //public bool fallJumpCancel = true;          // Air jump at downward velocity.
    //public bool terminalFallJumpCancel = false; // Air jump at terminal velocity.
    //public bool crouchJumpCancel = true;        // Crouch jump.
    //public bool dashJumpCancel = true;          // Dash jump preserves most momentum.
    //public bool attackJumpCancel = false;       // N/A

    //// into crouching
    //public bool jumpCrouchCancel = false;       // N/A
    //public bool fallCrouchCancel = true;        // Fastfall
    //public bool terminalFallCrouchCancel = false;   // Splat
    //public bool crouchCrouchCancel = true;      // Jump down through platforms.
    //public bool dashCrouchCancel = true;        // Slide under enemies. Slower than dash.
    //public bool attackCrouchCancel = false;     // N/A

    //// into dashing
    //public bool jumpDashCancel = true;          // Midair dash.
    //public bool fallDashCancel = true;          // Midair dash.
    //public bool terminalFallDashCancel = false;     // Maybe, but you'd keep vertical momentum.
    //public bool crouchDashCancel = true;        // Slide under enemies. Slower than dash.
    //public bool dashDashCancel = false;         // Leaving this off to force good timing.
    //public bool attackDashCancel = false;       // Maybe some attacks/weapons can do this.

    //// into attacking
    //public bool jumpAttackCancel = true;        // Midair attack.
    //public bool fallAttackCancel = true;        // Midair attack.
    //public bool terminalFallAttackCancel = true;    // Aerial Coup de Grace. Lethal on miss.
    //public bool crouchAttackCancel = true;      // Low attack/launcher.
    //public bool dashAttackCancel = true;        // Dash attack.
    //public bool attackAttackCancel = false;     // Attacks will chain based on buffered inputs.
    #endregion

    // Use this for initialization
    void Start()
    {
        #region Set up references.
        rb = GetComponent<Rigidbody2D>();
        hitbox = GetComponent<BoxCollider2D>();
        Transform groundTriggers = transform.Find("Ground_Triggers");
        groundCheck = groundTriggers.Find("Center").GetComponent<BoxCollider2D>();
        groundLeft = groundTriggers.Find("Left_Whisker").GetComponent<BoxCollider2D>();
        groundRight = groundTriggers.Find("Right_Whisker").GetComponent<BoxCollider2D>();
        groundMask = LayerMask.GetMask("Ground");
        #endregion
        attack("standingBasic",0f);
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
        //attackPress = Input.GetButtonDown("Attack");
        //attackHold = Input.GetButton("Attack");
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

        #region Action resolution
        if ((action != "none") && (actionStart + actionDur <= Time.frameCount))
        {
            endAction();
        }
        switch (action)
        {
            #region none
            case "none":
                if (jumpPress)
                {
                    if (grounded)
                    {
                        standingJump();
                    }
                    else if (jumps > 0)
                    {
                        fallingJump();
                        jumps--;
                    }
                }
                else if (dashPress)
                {

                }
                else if (attackPress)
                {
                    attack("standingBasic", 16f);
                }
                break;
            #endregion

            #region jumping
            case "jumping":
                if (jumpHold)
                {
                    GetComponent<Rigidbody2D>().AddForce(new Vector2(0f, jumpForceAdded));
                }
                else
                {
                    endAction();
                }
                break;
            #endregion

            #region dashing
            case "dashing":
                break;
            #endregion

            #region attacking
            case "attacking":
                break;
            #endregion
        }
        #endregion

        #region Movement
        Vector2 vel = rb.velocity;
        switch (action)
        {
            #region none
            case "none":
                // If the player is in motion but has no horizontal input, slow them down.
                if (hInput == 0 && rb.velocity.x != 0)
                    rb.AddForce(Vector2.right * (moveForce / 16) * Mathf.Sign(-vel.x));

                // If the player's horizontal velocity is less than max speed add force to the player based on input.
                else if (Mathf.Sign(hInput) * vel.x < maxSpeed)
                    if (grounded)
                    {
                        rb.AddForce(Vector2.right * hInput * moveForce);
                    }
                    else
                    {
                        rb.AddForce(Vector2.right * hInput * airForce);
                    }

                // If the player's horizontal velocity is greater than the maxSpeed set it equal to the maxSpeed.
                if (Mathf.Abs(vel.x) > maxSpeed)
                    rb.velocity = new Vector2(Mathf.Sign(vel.x) * maxSpeed, vel.y);

                break;
            #endregion

            #region jumping
            case "jumping":
                break;
            #endregion

            #region dashing
            case "dashing":
                break;
            #endregion

            #region attacking
            case "attacking":
                break;
            #endregion
        }
        #endregion
    }

    #region Action functions
    private void endAction()
    {
        action = "none";
        actionStart = Time.frameCount;
    }

    private void standingJump()
    {
        // Add a vertical force to the player.
        GetComponent<Rigidbody2D>().AddForce(new Vector2(0f, jumpForceInitial));

        action = "jumping";
        actionStart = Time.frameCount;
        actionDur = jumpDur;
    }

    private void fallingJump()
    {
        // Reset vertical velocity and add a vertical force to the player.
        Vector2 v = rb.velocity;
        v.y = 0f;
        rb.velocity = v;
        rb.AddForce(new Vector2(0f, jumpForceInitial));

        state = "jumping";
        stateStart = Time.frameCount;
    }

    private void standingDash()
    {
        action = "dashing";
        actionStart = Time.frameCount;
        actionDur = dashDur;
    }

    private void attack(string attackType, float attackDur)
    {
        string attackData = File.ReadAllText(Application.dataPath + "/Text/Player/Attacks/" + attackType + ".txt");
        GameObject attack = Instantiate(playerAttack, transform);
        attack.GetComponent<Player_Melee>().attackData = attackData;
    }
    #endregion

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