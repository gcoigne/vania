using UnityEngine;

public class playerMovement : MonoBehaviour
{
    #region References to objects
    private Transform playerBody;               // Reference to the player's body's transform.
    private Transform groundCheck;              // A position marking where to check if the player is grounded.
    private Rigidbody2D rb;                     // Reference to the player's physics object.
    private Animator anim;                      // Reference to the player's animator component.
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
    [HideInInspector]
    public bool grounded = false;               // Whether or not the player is in contact with the ground.
    #endregion

    #region State-dependant variables
    public string state = "standing";           // The player's current state.

    // Standing State
    public float moveForce = 365f;              // Amount of force added to move the player left and right.
    public float maxSpeed = 5f;                 // The fastest the player can travel in the x axis.
    [HideInInspector]
    public bool facingRight = true;             // For determining which way the player is currently facing.
    [HideInInspector]
    public bool standing = false;               // Whether or not the player is standing.

    // Jumping State
    public int maxJumps = 1;                    // Numer of times the player can jump before landing.
    public float jumpForceInitial = 500f;       // Amount of force added when the player jumps.
    public float jumpForceAdded = 50f;          // Amount of force added each frame while the player holds jump.
    public float jumpTime = 50f;                // Maximum amount of time(ms) a player can gain jump speed.
    [HideInInspector]
    public float jumpStartTime;                 // Time the current jump started.
    [HideInInspector]
    public int jumps = 0;                       // Number of jumps the player has available.
    [HideInInspector]
    public bool jumping = false;                // Whether or not the player is jumping.

    // Falling State
    [HideInInspector]
    public bool falling = false;                // Whether or not the player is falling.

    // Longfalling State
    public float longFallSpeed;                 // The player's terminal velocity (triggers longfall when reached).
    [HideInInspector]
    public bool longFalling = false;            // Whether or not the player is longfalling.

    // Crouching State
    [HideInInspector]
    public bool crouching = false;              // Whether or not the player is crouching.

    // Dashing State
    public int maxDashes = 1;                   // Number of times the player can dash before landing/waiting.
    public float dashSpeed = 20f;               // Speed the player travels while dashing.
    public float dashTime = 50f;                // Time(ms) a dash lasts.
    public float dashCooldown = 2000f;          // Time(ms) dashes take to cooldown.
    [HideInInspector]
    public float dashStartTime;                 // Time the current dash started.]
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

    // Whether or not a player may cancel from the first state into the second via input.
    // Some cancels lead to unique moves.
    // This section seems like baggage, as you'll need to develop custom actions individually. Its only real use is enabling cancels when the player
    // gains an ability, but it should be done on ability basis, not input basis. The current layout is long and clogs Inspector w/ public fields.

    // into jumping
    public bool jumpJumpCancel = true;          // Air jump before the first jump ends.
    public bool fallJumpCancel = true;          // Air jump at downward velocity.
    public bool longFallJumpCancel = false;     // Air jump at terminal velocity.
    public bool crouchJumpCancel = true;        // Crouch jump.
    public bool dashJumpCancel = true;          // Dash jump preserves most momentum.
    public bool attackJumpCancel = false;       // N/A

    // into crouching
    public bool jumpCrouchCancel = false;       // N/A
    public bool fallCrouchCancel = true;        // Fastfall
    public bool longFallCrouchCancel = false;   // Splat
    public bool crouchCrouchCancel = true;      // Jump down through platforms.
    public bool dashCrouchCancel = true;        // Slide under enemies. Slower than dash.
    public bool attackCrouchCancel = false;     // N/A

    // into dashing
    public bool jumpDashCancel = true;          // Midair dash.
    public bool fallDashCancel = true;          // Midair dash.
    public bool longFallDashCancel = false;     // Maybe, but you'd keep vertical momentum.
    public bool crouchDashCancel = true;        // Slide under enemies. Slower than dash.
    public bool dashDashCancel = false;         // Leaving this off to force good timing.
    public bool attackDashCancel = false;       // Maybe some attacks/weapons can do this.

    // into attacking
    public bool jumpAttackCancel = true;        // Midair attack.
    public bool fallAttackCancel = true;        // Midair attack.
    public bool longFallAttackCancel = true;    // Aerial Coup de Grace. Lethal on miss.
    public bool crouchAttackCancel = true;      // Low attack/launcher.
    public bool dashAttackCancel = true;        // Dash attack.
    public bool attackAttackCancel = false;     // Attacks will chain based on buffered inputs.
    #endregion

    // Use this for initialization
    void Start()
    {
        #region Set up references.
        playerBody = transform.Find("body");
        groundCheck = transform.Find("groundCheck");
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        #endregion
    }

    // Update is called once per frame. It is mainly used for caching user input and rendering.
    void Update()
    {
        #region Cache the user's input.
        hInput = Input.GetAxis("horizontal");
        vInput = Input.GetAxis("vertical");
        jumpPress = Input.GetButtonDown("Jump");
        jumpHold = Input.GetButton("Jump");
        dashPress = Input.GetButtonDown("Dash");
        dashHold = Input.GetButton("Dash");
        attackPress = Input.GetButtonDown("Attack");
        attackHold = Input.GetButton("Attack");
        #endregion
    }

    // FixedUpdate runs once each physics engine frame. It is mainly used for applying forces and other physics-related events.
    void FixedUpdate()
    {
        #region Physics variables
        // The player is grounded if a linecast to the groundcheck position hits anything on the ground layer.
        grounded = Physics2D.Linecast(transform.position, groundCheck.position, 1 << LayerMask.NameToLayer("ground"));
        #endregion

        #region State transitions
        if (grounded)
        {
            switch (state)
            {
                #region standing
                case "standing":
                    break;
                #endregion
                
                #region jumping
                case "jumping":
                    if (jumpPress)
                    {
                        if (jumpJumpCancel && jumps > 0)
                        {
                            // Set the Jump animator trigger parameter.
                            anim.SetTrigger("Jump");

                            //// Play a random Jump audio clip.
                            //int i = Random.Range(0, jumpClips.Length);
                            //AudioSource.PlayClipAtPoint(jumpClips[i], transform.position);

                            // Call Jump function
                        }
                    }

                    else if (dashPress)
                    {
                        if (jumpDashCancel && dashes > 0)
                        {
                            // Set the Dash animator trigger parameter.
                            anim.SetTrigger("Dash");

                            //// Play a random Dash audio clip.
                            //int i = Random.Range(0, DashClips.Length);
                            //AudioSource.PlayClipAtPoint(DashClips[i], transform.position);

                            // Call dash function.

                            // Transition from jumping state to dashing state.
                            jumping = false;
                            dashing = true;
                        }
                    }

                    else if (attackPress)
                    {
                        if (jumpAttackCancel && canAttack)
                        {
                            // Attack anim and audio stuff

                            // Call the appropriate attack function.

                            // Transition from jumping state to attacking state.
                            jumping = false;
                            attacking = true;
                        }
                    }

                    else
                    {
                        if (jumpHold && Time.time <= (jumpStartTime + jumpTime))
                        {
                            // Add jump force.
                        }
                        else
                        {
                            // Transition from jumping state to falling state.
                            jumping = false;
                            falling = true;
                        }

                        // Midair controls.
                    }
                    break;
                #endregion

                #region dashing
                case "dashing":
                    if (jumpPress)
                    {
                        if (dashJumpCancel && jumps > 0)
                        {
                            // Set the Jump animator trigger parameter.
                            anim.SetTrigger("Jump");

                            //// Play a random Jump audio clip.
                            //int i = Random.Range(0, jumpClips.Length);
                            //AudioSource.PlayClipAtPoint(jumpClips[i], transform.position);

                            // Call Jump function.

                            // Transition from dashing state to jumping state.
                            dashing = false;
                            jumping = true;
                        }
                    }

                    else if (dashPress)
                    {
                        if (dashDashCancel && dashes > 0)
                        {
                            // Set the Dash animator trigger parameter.
                            anim.SetTrigger("Dash");

                            //// Play a random Dash audio clip.
                            //int i = Random.Range(0, DashClips.Length);
                            //AudioSource.PlayClipAtPoint(DashClips[i], transform.position);

                            // Call Dash function.
                        }
                    }

                    else if (attackPress)
                    {
                        if (dashAttackCancel && canAttack)
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

                    else if (Time.time > (dashStartTime + dashTime))
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

                #region falling
                case "falling":
                    if (jumpPress)
                    {
                        if (fallJumpCancel && jumps > 0)
                        {
                            // Set the Jump animator trigger parameter.
                            anim.SetTrigger("Jump");

                            //// Play a random Jump audio clip.
                            //int i = Random.Range(0, jumpClips.Length);
                            //AudioSource.PlayClipAtPoint(jumpClips[i], transform.position);

                            // Call Jump function

                            // Transition from jumping state to dashing state.
                            falling = false;
                            jumping = true;
                        }
                    }

                    else if (dashPress)
                    {
                        if (fallDashCancel && dashes > 0)
                        {
                            // Set the Dash animator trigger parameter.
                            anim.SetTrigger("Dash");

                            //// Play a random Dash audio clip.
                            //int i = Random.Range(0, DashClips.Length);
                            //AudioSource.PlayClipAtPoint(DashClips[i], transform.position);

                            // Call dash function.

                            // Transition from falling state to dashing state.
                            falling = false;
                            dashing = true;
                        }
                    }

                    else if (attackPress)
                    {
                        if (fallAttackCancel && canAttack)
                        {
                            // Attack anim and audio stuff

                            // Call the appropriate attack function.

                            // Transition from falling state to attacking state.
                            falling = false;
                            attacking = true;
                        }
                    }

                    else
                    {
                        // Aerial controls
                    }
                    break;
                #endregion

                #region longFalling
                case "longFalling":
                    if (jumpPress)
                    {
                        if (longFallJumpCancel && jumps > 0)
                        {
                            // Set the Jump animator trigger parameter.
                            anim.SetTrigger("Jump");

                            //// Play a random Jump audio clip.
                            //int i = Random.Range(0, jumpClips.Length);
                            //AudioSource.PlayClipAtPoint(jumpClips[i], transform.position);

                            // Call Jump function

                            // Transition from longfalling state to jumping state.
                            longFalling = false;
                            jumping = true;
                        }
                    }

                    else if (dashPress)
                    {
                        if (longFallDashCancel && dashes > 0)
                        {
                            // Set the Dash animator trigger parameter.
                            anim.SetTrigger("Dash");

                            //// Play a random Dash audio clip.
                            //int i = Random.Range(0, DashClips.Length);
                            //AudioSource.PlayClipAtPoint(DashClips[i], transform.position);

                            // Call dash function.

                            // Transition from longfalling state to dashing state.
                            longFalling = false;
                            dashing = true;
                        }
                    }

                    else if (attackPress)
                    {
                        if (longFallAttackCancel && canAttack)
                        {
                            // Attack anim and audio stuff

                            // Call the appropriate attack function.

                            // Transition from longfalling state to attacking state.
                            longFalling = false;
                            attacking = true;
                        }
                    }

                    else
                    {
                        // Aerial controls.
                    }
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
                        standingBasicAttack();

                        // Attacking controls
                    }

                    else if (vInput < -0.5f)
                    {
                        standingCrouch();

                        // Crouching controls.
                    }

                    else
                    {

                    }

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

                #region falling
                case "falling":
                    break;
                #endregion

                #region longFalling
                case "longFalling":
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
            switch (state)
            {
                #region standing
                case "standing":
                    // The Speed animator parameter is set to the absolute value of the horizontal input.
                    anim.SetFloat("Speed", Mathf.Abs(hInput));

                    // If the player's horizontal velocity is less than max speed add force to the player based on input.
                    if (hInput * rb.velocity.x < maxSpeed)
                        rb.AddForce(Vector2.right * hInput * moveForce);

                    // If the player's horizontal velocity is greater than the maxSpeed set it equal to the maxSpeed.
                    if (Mathf.Abs(rb.velocity.x) > maxSpeed)
                        rb.velocity = new Vector2(Mathf.Sign(rb.velocity.x) * maxSpeed, rb.velocity.y);

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

                #region falling
                case "falling":
                    break;
                #endregion

                #region longFalling
                case "longFalling":
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

                #region falling
                case "falling":
                    break;
                #endregion

                #region longFalling
                case "longFalling":
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
    private void standingJump()
    {
        // Set the Jump animator trigger paramete
        anim.SetTrigger("Jump");

        //// Play a random jump audio clip.
        //int i = Random.Range(0, jumpClips.Length);
        //AudioSource.PlayClipAtPoint(jumpClips[i], transform.position);

        // Add a vertical force to the player.
        GetComponent<Rigidbody2D>().AddForce(new Vector2(0f, jumpForceInitial));
        state = "jumping";
    }

    private void midairJump()
    {
        // Set the Jump animator trigger paramete
        anim.SetTrigger("Jump");

        //// Play a random jump audio clip.
        //int i = Random.Range(0, jumpClips.Length);
        //AudioSource.PlayClipAtPoint(jumpClips[i], transform.position);

        // Add a vertical force to the player.
        Vector2 v = rb.velocity;
        v.y = 0f;
        rb.velocity = v;
        rb.AddForce(new Vector2(0f, jumpForceInitial));
        state = "jumping";
    }

    private void dashingJump()
    {

    }
    
    private void crouchingJump()
    {

    }


    private void standingFall()
    {

    }

    private void midairFall()
    {

    }

    private void longFall()
    {

    }


    private void standingCrouch()
    {

    }

    private void midairCrouch()
    {

    }


    private void standingDash()
    {
    
    }

    private void midairDash()
    {

    }

    private void crouchingDash()
    {

    }


    private void standingBasicAttack()
    {

    }


    private void Flip()
    {
        // Switch the way the player is labelled as facing.
        facingRight = !facingRight;

        // Multiply the player's x local scale by -1.
        Vector3 theScale = playerBody.localScale;
        theScale.x *= -1;
        playerBody.localScale = theScale;
    }
    #endregion
}