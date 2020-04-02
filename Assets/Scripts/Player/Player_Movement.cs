using UnityEngine;
using UnityEngine.InputSystem;

public class Player_Movement : MonoBehaviour
{
    #region References to objects
    private Animator anim;                      
    private Rigidbody2D rb;                     
    private BoxCollider2D hitbox;               
    private BoxCollider2D topTrigger;
    private BoxCollider2D frontTrigger;
    private BoxCollider2D bottomTrigger;
    private BoxCollider2D backTrigger;
    private BoxCollider2D topBackTrigger;
    private BoxCollider2D topFrontTrigger;
    private BoxCollider2D bottomFrontTrigger;
    private BoxCollider2D bottomBackTrigger;
    private GameObject gimpGun;
    private Transform gimpGunTip;

    private string[] checks;
    private string topCheck;
    private string frontCheck;
    private string bottomCheck;
    private string backCheck;
    private string topBackCheck;
    private string topFrontCheck;
    private string bottomFrontCheck;
    private string bottomBackCheck;

    public GameObject nullObject;

    public GameObject playerAttack;
    public GameObject dart;
    public GameObject dartDiag;

    private LayerMask groundMask;
    private LayerMask wallMask;
    private LayerMask climbableMask;
    #endregion

    #region Input variables
    private InputDevice controller;

    private float hInput;                       
    private float vInput;                       

    private bool jumpPress = false;    
    private bool jumpHold = false;     

    private bool dashPress = false;    
    private bool dashHold = false;     

    private bool attackPress = false;  
    private bool attackHold = false;   

    private bool aimPress = false;
    private bool aimHold = false;

    private bool interactPress = false;
    private bool interactHold = false; 
    #endregion

    #region Physics variables
    public bool grounded = false;
    public float gravScale = 1;
    public bool facingRight = true;
    #endregion

    #region State Variables
    public string state = "idle";              // States: idle, run, fall, jump, grip, climb, aim
    public float stateTimer = 0f;              // Time (frames) when the player switched into the current state.
    //private string action = "none";           // Player's current action.
    //private float actionStart = 0f;           // Time (frames) when the player started current action.
    //private float actionDur = 0f;             // Duration of current action.

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
    public int maxAirJumps = 1;                    // Numer of times the player can jump in midair before landing.
    public float jumpForceInitial = 60f;        // Amount of force added when the player jumps.
    public float jumpForceAdded = 1f;           // Amount of force added each frame while the player holds jump.
    public float jumpDur = 30f;                 // Maximum amount of time (frames) a player can gain jump speed.
    [HideInInspector]
    public int airJumps = 0;                       // Number of air jumps the player has available.

    [HideInInspector]
    public bool jumping = false;                // Whether or not the player is jumping.

    // Crouching State
    [HideInInspector]
    public bool crouching = false;              // Whether or not the player is crouching.

    // Climbing State
    [HideInInspector]
    public bool topFrontClimbable = false;      // Whether there is a climbable object in the top right trigger.
    [HideInInspector]
    public bool bottomFrontClimbable = false;   // Whether there is a climbable object in the top right trigger.


    [HideInInspector]
    public bool climbing = false;               // Whether or not the player is climbing.


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

    // Aim State
    public int aimDirection = 6;                // 8-direction numpad orientation
    #endregion

    #region Rendering Variables
    public int ppu = 16;        //Pixels per unit
    #endregion

    // Use this for initialization
    void Start()
    {
        #region Set up references.
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        hitbox = GetComponent<BoxCollider2D>();

        Transform triggers = transform.Find("Triggers");
        topTrigger = triggers.Find("Top").GetComponent<BoxCollider2D>();
        frontTrigger = triggers.Find("Front").GetComponent<BoxCollider2D>();
        bottomTrigger = triggers.Find("Bottom").GetComponent<BoxCollider2D>();
        backTrigger = triggers.Find("Back").GetComponent<BoxCollider2D>();
        topBackTrigger = triggers.Find("TopBack").GetComponent<BoxCollider2D>();
        topFrontTrigger = triggers.Find("TopFront").GetComponent<BoxCollider2D>();
        bottomFrontTrigger = triggers.Find("BottomFront").GetComponent<BoxCollider2D>();
        bottomBackTrigger = triggers.Find("BottomBack").GetComponent<BoxCollider2D>();

        gimpGun = transform.Find("GimpGun").gameObject;
        gimpGunTip = gimpGun.transform.Find("Tip");

        groundMask = LayerMask.GetMask("Ground");
        wallMask = LayerMask.GetMask("Wall");
        climbableMask = LayerMask.GetMask("Climbable");
        #endregion
    }

    // Update is called once per frame. It is mainly used for caching user input and rendering.
    void Update()
    {
        var gamepad = Gamepad.current;

        #region Cache the user's input.
        hInput = Input.GetAxisRaw("Horizontal");
        vInput = Input.GetAxisRaw("Vertical");
        jumpPress = Input.GetButtonDown("Jump");
        jumpHold = Input.GetButton("Jump");
        //dashPress = Input.GetButtonDown("Dash");
        //dashHold = Input.GetButton("Dash");
        attackPress = Input.GetButtonDown("Fire1");
        attackHold = Input.GetButton("Fire1");
        aimPress = Input.GetButtonDown("Fire1");
        aimHold = Input.GetButton("Fire1");
        interactPress = Input.GetButtonDown("Interact");
        interactHold = Input.GetButtonDown("Interact");
        #endregion

        #region State changes
        //if ((action != "none") && (actionStart + actionDur <= Time.frameCount))
        //{
        //    endAction();
        //}
        //if (interactPress)
        //{
        //    interact();
        //}
        switch (state)
        {
            #region idle
            case "idle":
                if (jumpPress)
                {
                    if (grounded)
                    {
                        idleToJump();
                    }
                }
                else if (attackPress)
                {
                    attack();
                }
                else if (topFrontClimbable && bottomFrontClimbable)
                {
                    //idleToClimb();
                }
                else if (rb.velocity.x != 0)
                {
                    idleToRun();
                }
                break;
            #endregion

            #region run
            case "run":
                if (jumpPress)
                {
                    if (grounded)
                    {
                        runToJump();
                    }
                }
                else if (attackPress)
                {
                    attack();
                }
                else if (topFrontClimbable && bottomFrontClimbable)
                {
                    //runToClimb();
                }
                else if (rb.velocity.x == 0)
                {
                    runToIdle();
                }
                break;
            #endregion

            #region fall
            case "fall":
                if (grounded)
                {
                    if (rb.velocity.x == 0)
                    {
                        fallToIdle();
                    }
                    else
                    {
                        fallToRun();
                    }
                }
                else
                {
                    if (topFrontCheck == "climbable" && bottomFrontCheck == "climbable")
                    {
                        fallToClimb();
                    }
                }
                break;
            #endregion

            #region jump
            case "jump":
                if (topFrontCheck == "climbable" && bottomFrontCheck == "climbable")
                {
                    //jumpToClimb();
                }
                else if (jumpHold && stateTimer + jumpDur > Time.frameCount)
                {
                    GetComponent<Rigidbody2D>().AddForce(new Vector2(0f, jumpForceAdded));
                }
                else
                {
                    jumpToFall();
                }
                break;
            #endregion

            case "grip":
                break;

            #region climb
            case "climb":
                if (jumpPress)
                {
                    climbToJump();
                    topFrontCheck = "";
                    bottomFrontCheck = "";
                }
                break;
            #endregion

            #region aim
            case "aim":
                if (hInput > 0.25)
                {
                    if (vInput > 0.25)
                    {
                        if (aimDirection != 9)
                        {
                            aimDirection = 9;
                            anim.SetTrigger("9");
                        }
                    }
                    else if (vInput < -0.25)
                    {
                        if (aimDirection != 3)
                        {
                            aimDirection = 3;
                            anim.SetTrigger("3");
                        }
                    }
                    else
                    {
                        if (aimDirection != 6)
                        {
                            aimDirection = 6;
                            anim.SetTrigger("6");
                        }
                    }
                }
                else if (hInput < -0.25)
                {
                    //turnAround(); no
                }
                else
                {
                    if (vInput > 0.25) 
                    {
                        if (aimDirection != 8)
                        {
                            aimDirection = 8;
                            anim.SetTrigger("8");
                        }
                    }
                    else if (vInput < -0.25)
                    {
                        if (aimDirection != 2)
                        {
                            aimDirection = 2;
                            anim.SetTrigger("2");
                        }
                    }
                }
                if (attackPress)
                {
                    attack();
                }
                break;
                #endregion
        }
        #endregion
    }

    // FixedUpdate runs once each physics engine frame. It is mainly used for applying forces and other physics-related events.
    private void FixedUpdate()
    {
        #region Physics variables
        Vector2 vel = rb.velocity;

        topCheck = checkTrigger(topTrigger);
        frontCheck = checkTrigger(frontTrigger);
        bottomCheck = checkTrigger(bottomTrigger);
        backCheck = checkTrigger(backTrigger);
        topBackCheck = checkTrigger(topBackTrigger);
        topFrontCheck = checkTrigger(topFrontTrigger);
        bottomFrontCheck = checkTrigger(bottomFrontTrigger);
        bottomBackCheck = checkTrigger(bottomBackTrigger);

        if (facingRight)
        {
            if (vel.x < 0)
            {
                turnAround();
            }
        }
        else
        {
            if (vel.x > 0)
            {
                turnAround();
            }
        }

        grounded = Physics2D.IsTouchingLayers(bottomTrigger, groundMask);
        if (grounded)
        {
            airJumps = maxAirJumps;
        }
        #endregion

        #region Movement
        switch (state)
        {
            #region idle
            case "idle":
                // If the player is in motion but has no horizontal input, slow them down.
                //if (hInput == 0 && rb.velocity.x != 0)
                    //rb.AddForce(Vector2.right * (moveForce / 16) * Mathf.Sign(-vel.x));

                // If the player's horizontal velocity is less than max speed add force to the player based on input.
                //else 
                if (Mathf.Sign(hInput) * vel.x < maxSpeed)
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

            #region run
            case "run":
                // If the player is in motion but has no horizontal input, slow them down.
                //if (hInput == 0 && rb.velocity.x != 0)
                //rb.AddForce(Vector2.right * (moveForce / 16) * Mathf.Sign(-vel.x));

                // If the player's horizontal velocity is less than max speed add force to the player based on input.
                //else 
                if (Mathf.Sign(hInput) * vel.x < maxSpeed)
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

            #region fall
            case "fall":
                // If the player is in motion but has no horizontal input, slow them down.
                //if (hInput == 0 && rb.velocity.x != 0)
                //rb.AddForce(Vector2.right * (moveForce / 16) * Mathf.Sign(-vel.x));

                // If the player's horizontal velocity is less than max speed add force to the player based on input.
                //else 
                if (Mathf.Sign(hInput) * vel.x < maxSpeed)
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
            
            #region jump
            case "jump":
                // If the player is in motion but has no horizontal input, slow them down.
                //if (hInput == 0 && rb.velocity.x != 0)
                //rb.AddForce(Vector2.right * (moveForce / 16) * Mathf.Sign(-vel.x));

                // If the player's horizontal velocity is less than max speed add force to the player based on input.
                //else 
                if (Mathf.Sign(hInput) * vel.x < maxSpeed)
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

            #region grip
            case "grip":
                break;
            #endregion

            #region climb
            case "climb":
                // If the player's vertical velocity is less than max speed add force to the player based on input.
                //else 
                if (Mathf.Sign(vInput) * vel.y < maxSpeed)
                    rb.velocity = Vector2.up * vInput * maxSpeed / 2;

                // If the player's horizontal velocity is greater than the maxSpeed set it equal to the maxSpeed.
                if (Mathf.Abs(vel.x) > maxSpeed)
                    rb.velocity = new Vector2(vel.x, Mathf.Sign(vel.y) * maxSpeed);

                if (vInput > 0)
                {
                    if (topFrontCheck == "climbable")
                    {
                        rb.velocity = Vector2.up * vInput * maxSpeed / 2;
                    }
                    else
                    {
                        rb.velocity = Vector2.zero;
                    }
                }

                else if (vInput < 0)
                {
                    if (bottomFrontCheck == "climbable")
                    {
                        rb.velocity = Vector2.up * vInput * maxSpeed / 2;
                    }
                    else
                    {
                        rb.velocity = Vector2.zero;
                    }
                }
                break;
            #endregion
        }
        #endregion

        #region Animation
        anim.SetFloat("hVel", rb.velocity.x);
        anim.SetFloat("vVel", rb.velocity.y);
        anim.SetBool("facingRight", facingRight);
        #endregion
    }

    private void LateUpdate()
    {
        transform.position = pixelClamp(transform.position);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        
    }

    #region Action functions
    private void endAction()
    {
        //action = "none";
        //actionStart = Time.frameCount;
    }

    private bool interact()
    {
        //Collider2D[] overlaps = new Collider2D[8];
        //ContactFilter2D cf = new ContactFilter2D();
        //cf.layerMask = new LayerMask();
        //if (hitbox.OverlapCollider(cf, overlaps) >= 1)
        //{
        //    foreach (Collider2D overlap in overlaps)
        //    {
        //        GameObject obj = overlap.gameObject;
        //        if (obj.tag == "Door")
        //        {
        //            if (overlap.gameObject.GetComponent<Door>().Interact())
        //            {
        //                return true;
        //            }
        //        }
        //    }
        //}
        return true;
    }

    #region to idle
    private void runToIdle()
    {
        state = "idle";
        stateTimer = Time.frameCount;
        anim.SetTrigger("Idle");
    }

    private void fallToIdle()
    {
        state = "idle";
        stateTimer = Time.frameCount;
        anim.SetTrigger("Idle");
    }

    private void jumpToIdle()
    {
        state = "idle";
        stateTimer = Time.frameCount;
        anim.SetTrigger("Idle");
    }

    private void gripToIdle()
    {
        state = "idle";
        stateTimer = Time.frameCount;
        anim.SetTrigger("Idle");
    }
    private void climbToIdle()
    {
        state = "idle";
        stateTimer = Time.frameCount;
        anim.SetTrigger("Idle");
    }
    #endregion

    #region to run
    private void idleToRun()
    {
        state = "run";
        stateTimer = Time.frameCount;
        anim.SetTrigger("Run");
    }

    private void jumpToRun()
    {
        state = "run";
        stateTimer = Time.frameCount;
        anim.SetTrigger("Run");
    }

    private void fallToRun()
    {
        state = "run";
        stateTimer = Time.frameCount;
        anim.SetTrigger("Run");
    }

    private void gripToRun()
    {
        state = "run";
        stateTimer = Time.frameCount;
        anim.SetTrigger("Run");
    }

    private void climbToRun()
    {
        state = "run";
        stateTimer = Time.frameCount;
        anim.SetTrigger("Run");
    }
    #endregion

    #region to fall
    private void idleToFall()
    {
        state = "fall";
        stateTimer = Time.frameCount;
        anim.SetTrigger("Fall");
    }
    private void runToFall()
    {
        state = "fall";
        stateTimer = Time.frameCount;
        anim.SetTrigger("Fall");
    }
    private void jumpToFall()
    {
        state = "fall";
        stateTimer = Time.frameCount;
        anim.SetTrigger("Fall");
    }
    private void gripToFall()
    {
        state = "fall";
        stateTimer = Time.frameCount;
        anim.SetTrigger("Fall");
    }
    private void climbToFall()
    {
        state = "fall";
        stateTimer = Time.frameCount;
        anim.SetTrigger("Fall");
    }

    #endregion

    #region to jump
    private void idleToJump()
    {
        // Add a vertical force to the player.
        GetComponent<Rigidbody2D>().AddForce(new Vector2(0f, jumpForceInitial));

        state = "jump";
        stateTimer = Time.frameCount;
        anim.SetTrigger("Jump");
    }
    private void runToJump()
    {
        // Add a vertical force to the player.
        GetComponent<Rigidbody2D>().AddForce(new Vector2(0f, jumpForceInitial));

        state = "jump";
        stateTimer = Time.frameCount;
        anim.SetTrigger("Jump");
    }

    private void fallToJump()
    {
        // Reset vertical velocity and add a vertical force to the player.
        rb.velocity = new Vector2(rb.velocity.x, 0);
        rb.AddForce(new Vector2(0f, jumpForceInitial));

        state = "jump";
        stateTimer = Time.frameCount;
        anim.SetTrigger("Jump");
    }
    
    private void gripToJump()
    {
        state = "jump";
        stateTimer = Time.frameCount;
        anim.SetTrigger("Jump");
    }

    private void climbToJump()
    {
        // Add a vertical force to the player.
        GetComponent<Rigidbody2D>().AddForce(new Vector2(-jumpForceInitial * Mathf.Cos(45f), jumpForceInitial * Mathf.Sin(45f)));
        rb.gravityScale = 1;
        turnAround();
        //resetChecks();

        state = "jump";
        stateTimer = Time.frameCount;
        anim.SetTrigger("Jump");
    }

    #endregion

    #region to grip
    private void idleToGrip()
    {
        state = "grip";
        stateTimer = Time.frameCount;
        anim.SetTrigger("Grip");
    }

    private void runToGrip()
    {
        state = "grip";
        stateTimer = Time.frameCount;
        anim.SetTrigger("Grip");
    }

    private void fallToGrip()
    {
        state = "grip";
        stateTimer = Time.frameCount;
        anim.SetTrigger("Grip");
    }

    private void jumpToGrip()
    {
        state = "grip";
        stateTimer = Time.frameCount;
        anim.SetTrigger("Grip");
    }

    private void climbToGrip()
    {
        state = "grip";
        stateTimer = Time.frameCount;
        anim.SetTrigger("Grip");
    }

    #endregion

    #region to climb
    private void idleToClimb()
    {
        state = "climb";
        stateTimer = Time.frameCount;
        anim.SetTrigger("Climb");
    }

    private void runToClimb()
    {
        state = "climb";
        stateTimer = Time.frameCount;
        anim.SetTrigger("Climb");
    }
    
    private void fallToClimb()
    {
        rb.gravityScale = 0;
        rb.velocity = Vector2.zero;

        state = "climb";
        stateTimer = Time.frameCount;
        anim.SetTrigger("Climb");
    }

    private void jumpToClimb()
    {
        rb.gravityScale = 0;
        rb.velocity = Vector2.zero;

        state = "climb";
        stateTimer = Time.frameCount;
        anim.SetTrigger("Climb");
    }

    private void gripToClimb()
    {
        state = "climb";
        stateTimer = Time.frameCount;
        anim.SetTrigger("Climb");
    }
    #endregion

    private void attack()
    {
        GameObject att  = nullObject;
        Dart dartScript;
        switch (aimDirection)
        {
            case 2:
                att = Instantiate(dart, gimpGunTip.position , Quaternion.Euler(0f, 0f, -90f));
                dartScript = att.GetComponent<Dart>();
                dartScript.angle = -90f;
                break;

            case 3:
                att = Instantiate(dartDiag, gimpGunTip.position, Quaternion.Euler(0f, 0f, -90f));
                dartScript = att.GetComponent<Dart>();
                dartScript.angle = -45f;
                break;

            case 6:
                att = Instantiate(dart, gimpGunTip.position, Quaternion.identity);
                dartScript = att.GetComponent<Dart>();
                dartScript.angle = 0f;
                break;

            case 8:
                att = Instantiate(dart, gimpGunTip.position, Quaternion.Euler(0f, 0f, 90f));
                dartScript = att.GetComponent<Dart>();
                dartScript.angle = 90f;
                break;

            case 9:
                att = Instantiate(dartDiag, gimpGunTip.position, Quaternion.identity);
                dartScript = att.GetComponent<Dart>();
                dartScript.angle = 45f;
                break;
        }
    }
    #endregion

    private void turnAround()
    {
        facingRight = !facingRight;

        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
    }

    //Returns the layer of the first valid collider in the trigger.
    private string checkTrigger(BoxCollider2D bc)
    {
        if (Physics2D.IsTouchingLayers(bc, groundMask))
        {
            return "ground";
        }
        if (Physics2D.IsTouchingLayers(bc, climbableMask))
        {
            return "climbable";
        }
        return "";
    }

    private void resetChecks()
    {
        topCheck = "";
        frontCheck = "";
        bottomCheck = "";
        backCheck = "";
        topBackCheck = "";
        topFrontCheck = "";
        bottomFrontCheck = "";
        bottomBackCheck = "";
    }

    // Clamps the player's position to the nearest pixel.
    private Vector2 pixelClamp(Vector2 v)
    {
        Vector2 pixelVector = new Vector2(Mathf.RoundToInt(v.x * ppu), Mathf.RoundToInt(v.y * ppu));
        return pixelVector / ppu;
    }
}