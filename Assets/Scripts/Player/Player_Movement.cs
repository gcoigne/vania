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
    private string state = "standing";         // Current physics state.
    private float stateTime = 0f;              // Time (frames) when the player switched into the current state. 

    // Standing State
    public float moveForce = 365f;              // Amount of force added to move the player left and right.
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
    public float jumpTime = 30f;                // Maximum amount of time (frames) a player can gain jump speed.
    public int jumps = 0;                       // Number of air jumps the player has available.
    [HideInInspector]
    public bool jumping = false;                // Whether or not the player is jumping.

    // Crouching State
    [HideInInspector]
    public bool crouching = false;              // Whether or not the player is crouching.

    // Dashing State
    public int maxDashes = 1;                   // Number of times the player can dash before landing/waiting.
    public float dashSpeed = 20f;               // Speed the player travels while dashing.
    public float dashTime = 50f;                // Time(ms) a dash lasts.
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

        #region State transitions
        if (grounded)
        {   
            switch (state)
            {
                #region standing
                case "standing":
                    if (jumpPress)
                    {
                        standingJump();
                    }
                    else if (dashPress)
                    {
                        standingDash();
                    }
                    else if (attackPress)
                    {
                        attack("standingBasic");
                    }
                    break;
                #endregion

                #region falling
                case "falling":
                    if (jumpPress)
                    {

                    }

                    else if (dashPress)
                    {
                        if (dashes > 0)
                        {

                        }
                    }

                    else if (attackPress)
                    {
                        if (canAttack)
                        {

                        }
                    }

                    else
                    {
                        // Landing
                        fallingStand();
                    }
                    break;
                #endregion

                #region terminalFalling
                case "terminalFalling":
                    if (jumpPress)
                    {
                        if (jumps > 0)
                        {

                        }
                    }

                    else if (dashPress)
                    {
                        if (dashes > 0)
                        {

                        }
                    }

                    else if (attackPress)
                    {
                        if (canAttack)
                        {

                        }
                    }

                    else
                    {

                    }
                    break;
                #endregion

                #region jumping
                case "jumping":
                    if (jumpPress)
                    {
                        if (jumps > 0)
                        {
                            jumpingJump();
                        }
                    }

                    else if (dashPress)
                    {
                        if (dashes > 0)
                        {
                            standingDash();
                        }
                    }

                    else if (attackPress)
                    {
                        if (canAttack)
                        {
                        }
                    }

                    else
                    {
                        if (jumpHold && Time.frameCount <= (stateTime + jumpTime))
                        {
                            GetComponent<Rigidbody2D>().AddForce(new Vector2(0f, jumpForceAdded));
                        }
                        else
                        {
                            // Transition from jumping to falling
                            jumpingFall();
                        }
                    }
                    break;
                #endregion

                #region dashing
                case "dashing":
                    if (jumpPress)
                    {
                        if (jumps > 0)
                        {
                            // Transition from dashing to jumping.
                            dashingJump();
                        }
                    }

                    else if (dashPress)
                    {
                        if (dashes > 0)
                        {
                            // Call Dash function.
                            dashingDash();
                        }
                    }

                    else if (attackPress)
                    {
                        if (canAttack)
                        {
                            // Call appropriate attack function.

                            // Transition from dashing state to attacking state.
                            dashing = false;
                            attacking = true;
                        }
                    }

                    else if (vInput < -.5f && standing)
                    {
                        // Call slide function.
                    }

                    else if (Time.frameCount > (stateTime + dashTime))
                    {
                        dashing = false;
                    }

                    else
                    {
                        if (facingRight)
                            rb.velocity = new Vector2(dashSpeed, 0);
                        else
                            rb.velocity = new Vector2(-dashSpeed, 0);
                    }
                    break;
                #endregion

                #region attacking
                case "attacking":
                    break;
                #endregion

                #region crouching
                case "crouching":
                    break;
                #endregion

                #region default
                default:
                    break;
                    #endregion
            }
        }

        else
        {
            switch (state)
            {
                #region standing
                case "standing":

                    if (jumpPress && jumps > 0)
                    {
                        if (vInput < 0.5f)
                        {
                            crouchingJump();
                        }

                        else
                        {
                            standingJump();
                        }

                        // Jumping controls
                    }

                    else if (dashPress && dashes > 0)
                    {
                        if (vInput < 0.5f)
                        {
                            crouchingDash();
                        }

                        else
                        {
                            standingDash();
                        }

                        // Dashing controls
                    }

                    else if (attackPress && canAttack)
                    {
                        attack("standingBasic", 16f);
                    }

                    else if (vInput < -0.5f)
                    {
                        standingCrouch();

                        // Crouching controls.
                    }

                    else
                    {
                        standingFall();
                    }

                    break;
                #endregion

                #region falling
                case "falling":
                    if (jumpPress && jumps > 0)
                    {
                        // Transition from falling to jumping.
                        fallingJump();
                        jumps--;
                    }
                    break;
                #endregion

                #region terminalFalling
                case "terminalFalling":
                    break;
                #endregion

                #region jumping
                case "jumping":
                    if (jumpPress && jumps > 0)
                    {
                        // Transition from standing to jumping.
                        jumpingJump();
                        jumps--;
                    }
                    else if (jumpHold && Time.frameCount <= (stateTime + jumpTime))
                    {
                        GetComponent<Rigidbody2D>().AddForce(new Vector2(0f, jumpForceAdded));
                    }
                    else
                    {
                        // Transition from jumping to falling
                        jumpingFall();
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

                #region crouching
                case "crouching":
                    break;
                #endregion

                #region default
                default:
                    break;
                    #endregion
            }
        }
        #endregion

        #region Movement
        if (grounded)
        {
            rb.gravityScale = 0;

            switch (state)
            {
                #region standing
                case "standing":

                    float vx = rb.velocity.x;

                    // If the player is in motion but has no horizontal input, slow them down.
                    if (hInput == 0 && rb.velocity.x != 0) 
                        rb.AddForce(Vector2.right * (moveForce / 16) * Mathf.Sign(-vx));

                    // If the player's horizontal velocity is less than max speed add force to the player based on input.
                    else if (Mathf.Sign(hInput) * vx < maxSpeed)
                        rb.AddForce(Vector2.right * hInput * moveForce);

                    // If the player's horizontal velocity is greater than the maxSpeed set it equal to the maxSpeed.
                    if (Mathf.Abs(rb.velocity.x) > maxSpeed)
                        rb.velocity = new Vector2(Mathf.Sign(rb.velocity.x) * maxSpeed, rb.velocity.y);

                    break;
                #endregion

                #region falling
                case "falling":
                    break;
                #endregion

                #region terminalFalling
                case "terminalFalling":
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

                #region crouching
                case "crouching":
                    break;
                #endregion

                #region default
                default:
                    break;
                    #endregion
            }
        }

        else
        {
            rb.gravityScale = gravScale;

            switch (state)
            {
                #region standing
                case "standing":
                    break;
                #endregion

                #region falling
                case "falling":

                    // If the player's horizontal velocity is less than max speed add force to the player based on input.
                    if (hInput * rb.velocity.x < maxSpeed)
                        rb.AddForce(Vector2.right * hInput * moveForce / 4);

                    // If the player's horizontal velocity is greater than the maxSpeed set it equal to the maxSpeed.
                    if (Mathf.Abs(rb.velocity.x) > maxSpeed)
                        rb.velocity = new Vector2(Mathf.Sign(rb.velocity.x) * maxSpeed, rb.velocity.y);

                    break;
                #endregion

                #region terminalFalling
                case "terminalFalling":
                    break;
                #endregion

                #region jumping
                case "jumping":

                    // If the player's horizontal velocity is less than max speed add force to the player based on input.
                    if (hInput * rb.velocity.x < maxSpeed)
                        rb.AddForce(Vector2.right * hInput * moveForce / 4);

                    // If the player's horizontal velocity is greater than the maxSpeed set it equal to the maxSpeed.
                    if (Mathf.Abs(rb.velocity.x) > maxSpeed)
                        rb.velocity = new Vector2(Mathf.Sign(rb.velocity.x) * maxSpeed, rb.velocity.y);

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

                #region crouching
                case "crouching":
                    break;
                #endregion

                #region default
                default:
                    break;
                    #endregion
            }
        }
        #endregion
    }

    #region State transition functions.
    private void fallingStand()
    {
        rb.gravityScale = 0;

        state = "standing";
        stateTime = Time.frameCount;
    }

    private void terminalFallingStand() { }

    private void jumpingStand() { }

    private void dashingStand() { }

    private void crouchingStand() { }


    private void standingJump()
    {
        // Add a vertical force to the player.
        GetComponent<Rigidbody2D>().AddForce(new Vector2(0f, jumpForceInitial));

        state = "jumping";
        stateTime = Time.frameCount;
    }

    private void fallingJump()
    {
        // Reset vertical velocity and add a vertical force to the player.
        Vector2 v = rb.velocity;
        v.y = 0f;
        rb.velocity = v;
        rb.AddForce(new Vector2(0f, jumpForceInitial));

        state = "jumping";
        stateTime = Time.frameCount;
    }

    private void terminalFallingJump() { }

    private void jumpingJump()
    {
        // Reset vertical velocity and add a vertical force to the player.
        Vector2 v = rb.velocity;
        v.y = 0f;
        rb.velocity = v;
        rb.AddForce(new Vector2(0f, jumpForceInitial));

        state = "jumping";
        stateTime = Time.frameCount;
    }

    private void dashingJump()
    {

    }

    private void crouchingJump()
    {

    }


    private void standingFall()
    {
        state = "falling";
        stateTime = Time.frameCount;
    }

    private void terminalFall()
    {

    }

    private void jumpingFall()
    {
        state = "falling";
        stateTime = Time.frameCount;
    }

    private void dashingFall() { }

    private void crouchingFall() { }


    private void standingCrouch()
    {

    }

    private void fallingCrouch()
    {

    }

    private void terminalFallingCrouch() { }

    private void jumpingCrouch() { }

    private void dashingCrouch() { }


    private void standingDash()
    {
    
    }

    private void fallingDash() { }

    private void terminalFallingDash() { }

    private void jumpingDash()
    {

    }

    private void dashingDash() { }

    private void crouchingDash()
    {

    }

    // Attacks are handled in another script. We just pass parameters to inform that script of our state.
    private void attack(string attackType, float attackTime)
    {
        string attackData = File.ReadAllText(Application.dataPath + "/Assets/Text/Player/Attacks" + attackType);
        GameObject attack = Instantiate(playerAttack, transform);
        attack.GetComponent<Player_Melee>().attackData = attackData;
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
    #endregion
}