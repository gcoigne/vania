using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    #region References to objects                  
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

    private GameObject body;
    private GameObject arm;
    private GameObject gimpGun;
    private Transform gimpGunTip;

    private Animator anim;
    private Animator armAnim;
    private string[] animTriggers;
    private string[] armAnimTriggers;

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
    private LayerMask smoothMask;
    private LayerMask roughMask;
    //private string[] 
    #endregion

    #region Input variables
    private PlayerInput input;

    private float hInput;                       
    private float vInput;     

    private bool jumpPress = false;    
    private bool jumpHold = false;     

    private bool attackPress = false;  
    private bool attackHold = false;   

    private bool altPress = false;
    private bool altHold = false;

    private bool interactPress = false;
    private bool interactHold = false;

    private float inputAngle;                   // Rounded to the nearest 45
    private float aimAngle;                     // = inputAngle + facingAngle (flipped = inputAngle - facingAngle)
    #endregion

    #region Physics variables
    public float gravScale = 1f;
    public bool grounded = false;
    public float facingAngle = 0f;
    public bool flipped = false;

    private Vector2 remVector = Vector2.zero;   // Remainder from snapping to nearest pixel
    #endregion

    #region State Variables
    public string state = "idle";               // States: idle, run, fall, jump, grip, climb, hang, aim
    public float stateTimer = 0f;               // Time (frames) when the player switched into the current state.

    public float moveForce = 256f;              // Amount of force added to move the player left and right.
    public float airForce = 64f;                // Amount of horizontal force added while midair.
    public float maxSpeed = 5f;                 // The fastest the player can travel in the x axis.


    public int maxAirJumps = 1;                    // Numer of times the player can jump in midair before landing.
    public float jumpForceInitial = 60f;        // Amount of force added when the player jumps.
    public float jumpForceAdded = 1f;           // Amount of force added each frame while the player holds jump.
    public float jumpDur = 30f;                 // Maximum amount of time (frames) a player can gain jump speed.
    [HideInInspector]
    public int airJumps = 0;                       // Number of air jumps the player has available.

    [HideInInspector]
    public bool topFrontClimbable = false;      // Whether there is a climbable object in the top right trigger.
    [HideInInspector]
    public bool bottomFrontClimbable = false;   // Whether there is a climbable object in the top right trigger.


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

        body = transform.Find("Body").gameObject;
        arm = body.transform.Find("Arm").gameObject;
        gimpGun = arm.transform.Find("GimpGun").gameObject;
        gimpGunTip = gimpGun.transform.Find("Tip");

        //input = GetComponent<PlayerInput>();

        anim = body.GetComponent<Animator>();
        armAnim = arm.GetComponent<Animator>();

        animTriggers = new string[16];
        var i = 0;
        foreach (AnimatorControllerParameter param in anim.parameters)
        {
            if (param.type == AnimatorControllerParameterType.Trigger)
            {
                animTriggers[i] = param.name;
                i++;
                if (i == animTriggers.Length)
                    break;
            }
        }

        armAnimTriggers = new string[16];
        i = 0;
        foreach (AnimatorControllerParameter param in armAnim.parameters)
        {
            if (param.type == AnimatorControllerParameterType.Trigger)
            {
                armAnimTriggers[i] = param.name;
                i++;
                if (i == armAnimTriggers.Length)
                    break;
            }
        }

        groundMask = LayerMask.GetMask("Ground");
        smoothMask = LayerMask.GetMask("Smooth");
        roughMask = LayerMask.GetMask("Rough");
        #endregion
    }

    void Update()
    {
        #region Buttons
        hInput = Input.GetAxis("Horizontal");
        vInput = Input.GetAxis("Vertical");
        jumpPress = Input.GetButtonDown("Jump");
        jumpHold = Input.GetButton("Jump");
        //dashPress = Input.GetButtonDown("Dash");
        //dashHold = Input.GetButton("Dash");
        attackPress = Input.GetButtonDown("Fire");
        attackHold = Input.GetButton("Fire");
        altPress = Input.GetButtonDown("Alt");
        altHold = Input.GetButton("Alt");
        interactPress = Input.GetButtonDown("Interact");
        interactHold = Input.GetButtonDown("Interact");
        #endregion

        #region inputAngle
        if (hInput <= -.5f)
        {
            if (vInput <= -.5f)
            {
                inputAngle = 225f;
            }
            else if (vInput > .5f)
            {
                inputAngle = 135f;
            }
            else
            {
                inputAngle = 180f;
            }
        }
        else if (hInput >= .5f)
        {
            if (vInput <= -.5f)
            {
                inputAngle = 315f;
            }
            else if (vInput >= .5f)
            {
                inputAngle = 45f;
            }
            else
            {
                inputAngle = 0f;
            }
        }
        else
        {
            if (vInput <= -.5f)
            {
                inputAngle = 270f;
            }
            else if (vInput >= .5f)
            {
                inputAngle = 90f;
            }
            else
            {
                inputAngle = 0f;
            }
        }
        #endregion

        #region Actions
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
                else if (altHold)
                {
                    idleToAlt();
                }
                else if (Mathf.Abs(hInput) >= 0.25f)
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
                else if (altHold)
                {
                    runToAlt();
                }
                else if (Mathf.Abs(hInput) < 0.25f)
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
                if (attackPress)
                {
                    attack();
                }
                else
                {
                    if (topFrontCheck == "rough" && bottomFrontCheck == "rough")
                    {
                        fallToGrip();
                    }
                }
                break;
            #endregion
        
            #region jump
            case "jump":
                if (topFrontCheck == "rough" && bottomFrontCheck == "rough")
                {
                    jumpToClimb();
                    break;
                }
                if (jumpHold && stateTimer + jumpDur > Time.frameCount)
                {
                    GetComponent<Rigidbody2D>().AddForce(new Vector2(0f, jumpForceAdded));
                }
                else
                {
                    jumpToFall();
                }
                if (attackPress)
                {
                    attack();
                }
                break;
            #endregion

            #region grip
            case "grip":
                if (jumpPress)
                {
                    gripToJump();
                }
                else if (attackPress)
                {
                    attack();
                }
                else if (altHold)
                {
                    gripToClimbAlt();
                }
                else if (Mathf.Abs(vInput) >= 0.25)
                {
                    gripToClimb();
                }
                break;
            #endregion

            #region climb
            case "climb":
                if (jumpPress)
                {
                    climbToJump();
                }
                else if (attackPress)
                {
                    attack();
                }
                else if (altHold)
                {
                    climbToClimbAlt();
                }
                else if (Mathf.Abs(vInput) < 0.25)
                {
                    climbToGrip();
                }
                break;
            #endregion
        
            #region alt
            case "alt":
                if (altHold)
                {
                    if (attackPress)
                    {
                        attack();
                    }
                }
                else
                {
                    aimDirection = 6;
                    if (Mathf.Abs(hInput) < 0.25)
                    {
                        altToIdle();
                    }
                    else
                    {
                        altToRun();
                    }
                }
                break;
            #endregion

            #region climbAlt
            case "climbAlt":
                if (altHold)
                {
                    if (attackPress)
                    {
                        attack();
                    }
                }
                else
                {
                    aimDirection = 6;
                    if (Mathf.Abs(hInput) < 0.25 && Mathf.Abs(vInput) < 0.25)
                    {
                        climbAltToGrip();
                    }
                    else
                    {
                        climbAltToClimb();
                    }
                }
                break;
                #endregion
        }
        #endregion
    }

    private void FixedUpdate()
    {
        //transform.Translate(remVector);

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
                break;
            #endregion

            #region run
            case "run":
                // If the player is in motion but has no horizontal input, slow them down.
                //if (hInput == 0 && rb.velocity.x != 0)
                //rb.AddForce(Vector2.right * (moveForce / 16) * Mathf.Sign(-vel.x));

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

                if (facingAngle == 0)
                {
                    if (vel.x < 0)
                    {
                        turnAround();
                    }
                }
                else if (facingAngle == 180)
                {
                    if (vel.x > 0)
                    {
                        turnAround();
                    }
                }
                break;
            #endregion

            #region fall
            case "fall":
                if (Mathf.Sign(hInput) * vel.x < maxSpeed)
                        rb.AddForce(Vector2.right * hInput * airForce);

                // If the player's horizontal velocity is greater than the maxSpeed set it equal to the maxSpeed.
                if (Mathf.Abs(vel.x) > maxSpeed)
                    rb.velocity = new Vector2(Mathf.Sign(vel.x) * maxSpeed, vel.y);
                break;
            #endregion  
            
            #region jump
            case "jump":
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
                if (vInput > 0)
                {
                    if (facingAngle == 270f)
                    {
                        turnAround();
                    }
                    if (bottomFrontCheck == "rough")
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
                    if (facingAngle == 90f)
                    {
                        turnAround();
                    }
                    if (bottomBackCheck == "rough")
                    {
                        rb.velocity = Vector2.up * vInput * maxSpeed / 2;
                    }
                    else
                    {
                        rb.velocity = Vector2.zero;
                    }
                }

                else
                {
                    climbToGrip();
                }

                break;
            #endregion

            #region alt
            case "alt":
                aim();
                break;
            #endregion

            #region climbAlt
            case "climbAlt":
                aim();
                break;
            #endregion
        }
        #endregion

        #region Animation
        anim.SetFloat("hVel", rb.velocity.x);
        anim.SetFloat("vVel", rb.velocity.y);
        anim.SetFloat("facingAngle", facingAngle);
        #endregion

        Vector2 pixelVector = new Vector2(Mathf.RoundToInt(transform.position.x * ppu), Mathf.RoundToInt(transform.position.y * ppu)) / ppu;
        remVector = transform.position - (Vector3) pixelVector;
        //Debug.Log(remVector);

        //transform.position = pixelVector;
    }

    private void LateUpdate()
    {
        //transform.position = pixelClamp(transform.position);

        Vector2 pixelVector = new Vector2(Mathf.RoundToInt(transform.position.x * ppu), Mathf.RoundToInt(transform.position.y * ppu)) / ppu;
        //remVector = new Vector2(transform.position.x % ppu, transform.position.y % ppu);
        //
        body.transform.position = pixelVector;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        
    }

    #region Action functions

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
        switchAnimation("Idle");
    }

    private void fallToIdle()
    {
        state = "idle";
        stateTimer = Time.frameCount;
        switchAnimation("Idle");
    }

    private void jumpToIdle()
    {
        state = "idle";
        stateTimer = Time.frameCount;
        switchAnimation("Idle");
    }

    private void gripToIdle()
    {
        state = "idle";
        stateTimer = Time.frameCount;
        switchAnimation("Idle");
    }
    private void climbToIdle()
    {
        state = "idle";
        stateTimer = Time.frameCount;
        switchAnimation("Idle");
    }
    private void altToIdle()
    {
        state = "idle";
        stateTimer = Time.frameCount;
        switchAnimation("Idle");
    }
    #endregion

    #region to run
    private void idleToRun()
    {
        state = "run";
        stateTimer = Time.frameCount;
        switchAnimation("Run");
    }

    private void jumpToRun()
    {
        state = "run";
        stateTimer = Time.frameCount;
        switchAnimation("Run");
    }

    private void fallToRun()
    {
        state = "run";
        stateTimer = Time.frameCount;
        switchAnimation("Run");
    }

    private void gripToRun()
    {
        state = "run";
        stateTimer = Time.frameCount;
        switchAnimation("Run");
    }

    private void climbToRun()
    {
        state = "run";
        stateTimer = Time.frameCount;
        switchAnimation("Run");
    }

    private void altToRun()
    {
        state = "run";
        stateTimer = Time.frameCount;
        switchAnimation("Run");
    }
    #endregion

    #region to fall
    private void idleToFall()
    {
        state = "fall";
        stateTimer = Time.frameCount;
        switchAnimation("Fall");
    }
    private void runToFall()
    {
        state = "fall";
        stateTimer = Time.frameCount;
        switchAnimation("Fall");
    }
    private void jumpToFall()
    {
        state = "fall";
        stateTimer = Time.frameCount;
        switchAnimation("Fall");
    }
    private void gripToFall()
    {
        state = "fall";
        stateTimer = Time.frameCount;
        switchAnimation("Fall");
    }
    private void climbToFall()
    {
        state = "fall";
        stateTimer = Time.frameCount;
        switchAnimation("Fall");
    }

    #endregion

    #region to jump
    private void idleToJump()
    {
        // Add a vertical force to the player.
        GetComponent<Rigidbody2D>().AddForce(new Vector2(0f, jumpForceInitial));

        state = "jump";
        stateTimer = Time.frameCount;
        switchAnimation("Jump");
    }
    private void runToJump()
    {
        // Add a vertical force to the player.
        GetComponent<Rigidbody2D>().AddForce(new Vector2(0f, jumpForceInitial));

        state = "jump";
        stateTimer = Time.frameCount;
        switchAnimation("Jump");
    }

    private void fallToJump()
    {
        // Reset vertical velocity and add a vertical force to the player.
        rb.velocity = new Vector2(rb.velocity.x, 0);
        rb.AddForce(new Vector2(0f, jumpForceInitial));

        state = "jump";
        stateTimer = Time.frameCount;
        switchAnimation("Jump");
    }
    
    private void gripToJump()
    {
        if (flipped)
        {
            if (facingAngle == 90f)
            {
                rotateBody(90f);
                turnAround();
            }
            else if (facingAngle == 270f)
            {
                rotateBody(270f);
            }
        }
        else
        {
            if (facingAngle == 90f)
            {
                rotateBody(270f);
                turnAround();
            }
            else if (facingAngle == 270f)
            {
                rotateBody(90f);
            }
        }

        rb.gravityScale = 1;

        if (flipped)
        {
            rb.AddRelativeForce(new Vector2(jumpForceInitial * Mathf.Cos(45f), jumpForceInitial * Mathf.Sin(45f)));
        }
        else
        {
            rb.AddRelativeForce(new Vector2(-jumpForceInitial * Mathf.Cos(45f), jumpForceInitial * Mathf.Sin(45f)));
        }

        state = "jump";
        stateTimer = Time.frameCount;
        switchAnimation("Jump");
    }

    private void climbToJump()
    {
        if (flipped)
        {
            if (facingAngle == 90f)
            {
                rotateBody(90f);
                turnAround();
            }
            else if (facingAngle == 270f)
            {
                rotateBody(270f);
            }
        }
        else
        {
            if (facingAngle == 90f)
            {
                rotateBody(270f);
                turnAround();
            }
            else if (facingAngle == 270f)
            {
                rotateBody(90f);
            }
        }

        rb.gravityScale = 1;
        
        if (flipped)
        {
            rb.AddRelativeForce(new Vector2(jumpForceInitial * Mathf.Cos(45f), jumpForceInitial * Mathf.Sin(45f)));
        }
        else
        {
            rb.AddRelativeForce(new Vector2(-jumpForceInitial * Mathf.Cos(45f), jumpForceInitial * Mathf.Sin(45f)));
        }

        state = "jump";
        stateTimer = Time.frameCount;
        switchAnimation("Jump");
    }

    #endregion

    #region to grip
    private void idleToGrip()
    {
        state = "grip";
        stateTimer = Time.frameCount;
        switchAnimation("Grip");
    }

    private void runToGrip()
    {
        state = "grip";
        stateTimer = Time.frameCount;
        switchAnimation("Grip");
    }

    private void fallToGrip()
    {
        if (flipped)
        {
            rotateBody(270f);
        }
        else
        {
            rotateBody(90f);
        }

        rb.gravityScale = 0;
        rb.velocity = Vector2.zero;

        state = "grip";
        stateTimer = Time.frameCount;
        switchAnimation("Grip");
    }

    private void jumpToGrip()
    {
        state = "grip";
        stateTimer = Time.frameCount;
        switchAnimation("Grip");
    }

    private void climbToGrip()
    {
        rb.velocity = Vector2.zero;

        state = "grip";
        stateTimer = Time.frameCount;
        switchAnimation("Grip");
    }
    private void climbAltToGrip()
    {
        state = "grip";
        stateTimer = Time.frameCount;
        switchAnimation("Grip");
    }

    #endregion

    #region to climb
    private void idleToClimb()
    {
        state = "climb";
        stateTimer = Time.frameCount;
        switchAnimation("Climb");
    }

    private void runToClimb()
    {
        state = "climb";
        stateTimer = Time.frameCount;
        switchAnimation("Climb");
    }
    
    private void fallToClimb()
    {

        rb.gravityScale = 0;
        rb.velocity = Vector2.zero;

        state = "climb";
        stateTimer = Time.frameCount;
        switchAnimation("Climb");
    }

    private void jumpToClimb()
    {
        state = "climb";
        stateTimer = Time.frameCount;
        switchAnimation("Climb");
    }

    private void gripToClimb()
    {
        state = "climb";
        stateTimer = Time.frameCount;
        switchAnimation("Climb");
    }

    private void climbAltToClimb()
    {
        state = "climb";
        stateTimer = Time.frameCount;
        switchAnimation("Climb");
    }
    #endregion

    #region to alt
    private void idleToAlt()
    {
        rb.velocity = Vector2.zero;

        state = "alt";
        stateTimer = Time.frameCount;
        switchAnimation("Alt");
    }

    private void runToAlt()
    {
        rb.velocity = Vector2.zero;

        state = "alt";
        stateTimer = Time.frameCount;
        switchAnimation("Alt");
    }
    #endregion

    #region to climbAlt
    private void gripToClimbAlt()
    {
        rb.velocity = Vector2.zero;

        state = "climbAlt";
        stateTimer = Time.frameCount;
        switchAnimation("ClimbAlt");
    }

    private void climbToClimbAlt()
    {
        rb.velocity = Vector2.zero;

        state = "climbAlt";
        stateTimer = Time.frameCount;
        switchAnimation("ClimbAlt");
    }
    #endregion

    private void attack()
    {
        GameObject att;
        Dart dartScript;

        float tempAngle;
        if (flipped)
        {
            tempAngle = facingAngle - aimAngle;
        }
        else
        {
            tempAngle = facingAngle + aimAngle;
        }

        att = Instantiate(dart, gimpGunTip.position, Quaternion.Euler(0f, 0f, tempAngle));
        dartScript = att.GetComponent<Dart>();
        dartScript.angle = tempAngle;
    }
    #endregion

    private void turnAround()
    {
        Vector3 scale = transform.localScale;
        transform.localScale = new Vector3(scale.x * -1, scale.y, scale.z);
        flipped = !flipped;

        facingAngle = Mathf.Round((facingAngle + 180) % 360);
    }

    private void rotateBody(float deltaAngle)
    {
        facingAngle += deltaAngle;
        //if (flipped)
        //{
        //    facingAngle = (facingAngle + deltaAngle + 180f) % 360;
        //}
        facingAngle = (facingAngle + 360) % 360;
        
        transform.Rotate(0f, 0f, deltaAngle);
    }

    private void aim()
    {
        float tempAngle = inputAngle - facingAngle;
        tempAngle = (tempAngle + 360) % 360;

        if (tempAngle > 90 && tempAngle < 270)
        {
            turnAround();
        }

        if (flipped)
        {
            aimAngle = -inputAngle + facingAngle;
        }
        else
        {
            aimAngle = inputAngle - facingAngle;
        }
        aimAngle = (aimAngle + 360) % 360;
        armAnim.SetInteger("Angle", (int) aimAngle);
    }


    //Returns the layer of the first valid collider in the trigger.
    private string checkTrigger(BoxCollider2D bc)
    {
        if (Physics2D.IsTouchingLayers(bc, groundMask))
        {
            return "ground";
        }
        if (Physics2D.IsTouchingLayers(bc, smoothMask))
        {
            return "smooth";
        }
        if (Physics2D.IsTouchingLayers(bc, roughMask))
        {
            return "rough";
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

    private void switchAnimation(string trigger)
    {
        foreach (string t in animTriggers)
        {
            anim.ResetTrigger(t);
        }
        anim.SetTrigger(trigger);
        armAnim.SetTrigger("6");
        armAnim.SetInteger("Angle", 0);
    }

    private void armAnimation(string trigger)
    {
        foreach (string t in armAnimTriggers)
        {
            armAnim.ResetTrigger(t);
        }
        armAnim.SetTrigger(trigger);
    }
}