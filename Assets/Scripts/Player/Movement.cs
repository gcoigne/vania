using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    #region References to objects
    private Animator anim;                      // Reference to the player's animator component.
    private Rigidbody2D rb;                     // Reference to the player's physics object.
    private BoxCollider2D hitbox;               // The player's main hitbox.
    private BoxCollider2D groundCheck;          // A hitbox that checks if the player is grounded.
    private BoxCollider2D groundLeft;
    private BoxCollider2D groundRight;
    private LayerMask groundMask;               // Reference to Ground layer mask.
    #endregion

    #region Input variables
    private float hInput;                       // Horizontal input.
    private float vInput;                       // Vertical input.

    private bool jumpPress = false;             // Whether or not the player is pressing jump.
    private bool jumpHold = false;              // Whether or not the player is holding jump.

    private bool interactPress = false;         // Whether or not the player is pressing interact.
    private bool interactHold = false;          // Whether or not the player is holding interact.
    #endregion

    #region Physics variables
    public float moveForce = 256f;              // Amount of force added to move the player left and right.
    public float airForce = 64f;                // Amount of horizontal force added while midair.
    public float maxSpeed = 5f;                 // The fastest the player can travel in the x axis.
    public float jumpForceInitial = 60f;        // Amount of force added when the player jumps.
    public float jumpForceAdded = 1f;           // Amount of force added each frame while the player holds jump.
    public float jumpMaxDur = 30f;              // Maximum amount of time (frames) a player can gain jump speed.

    private bool grounded = false;              // Whether the player is touching the ground.
    private bool facingRight = true;            // Whether the player is facing right.
    private string animState = "idle";          // The player's current animation.


    #endregion

    // Start is called before the first frame update
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

    // Update is called once per frame
    void Update()
    {
        #region Cache the user's input.
        hInput = Input.GetAxis("Horizontal");
        vInput = Input.GetAxis("Vertical");
        jumpPress = Input.GetButtonDown("Jump");
        jumpHold = Input.GetButton("Jump");
        interactPress = Input.GetButtonDown("Interact");
        interactHold = Input.GetButtonDown("Interact");
        #endregion
    }

    // FixedUpdate is called once per physics frame
    void FixedUpdate()
    {
        #region Physics
        animState = anim.GetCurrentAnimatorStateInfo(0).ToString();
        grounded = Physics2D.IsTouchingLayers(groundCheck, groundMask);
        #endregion

        #region Actions
        if (grounded)
        {
            if (interactPress)
            {
                //
            }

            else if (jumpPress)
            {
                jump();
            }
        }
        
        else
        {
            if (jumpHold);
        }
        #endregion
    }

    #region Actions
    private void jump()
    {
        // Add a vertical force to the player.
        GetComponent<Rigidbody2D>().AddForce(new Vector2(0f, jumpForceInitial));

        anim.SetTrigger("jump");
    }

    private void fall()
    {
        anim.SetTrigger("fall");
    }
    #endregion
}
