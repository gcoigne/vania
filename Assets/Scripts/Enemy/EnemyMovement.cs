using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour {

    #region References to objects
    private Rigidbody2D rb;                     // The enemy's physics component.
    private BoxCollider2D hitbox;               // The player's main hitbox.
    private BoxCollider2D groundCheck;          // A hitbox that checks if the enemy is grounded.
    private BoxCollider2D groundLeft;           // A hitbox that checks if there's ground to the enemy's left.
    private BoxCollider2D groundRight;          // A hitbox that checks if there's ground to the enemy's right.
    private Animator anim;                      // The enemy's animator component.
    private LayerMask groundMask;               // Reference to Ground layer mask.
    #endregion

    #region Physics variables
    public bool grounded = false;               // Whether or not the enemy is in contact with the ground.
    public bool groundedLeft = false;           // Whether or not there is ground to the left.
    public bool groundedRight = false;          // Whether or not there is ground to the right.
    #endregion

    #region State and Behavior Variables
    public string state = "standing";           // Current physics state.
    public float stateTime = 0f;                // Time (frames) when the enemy switched into this state.
    public string behavior = "none";            // Current behavior
    public float behaviorTime = 0f;             // Time (frames) when the enemy switched into this behavior.
    public float behaviorWait = 0f;             // Time (frames) the enemy waits when entering a new behavior.
    #endregion

    #region Movement Variables
    public float moveForce = 365f;              // Amount of force added to move the enemy left and right.
    public float maxSpeed = 5f;                 // The fastest the enemy can travel in the x axis.

    public int maxJumps = 1;                    // Numer of times the enemy can jump before landing.
    public float jumpForce = 60f;               // Amount of force added when the enemy jumps.
    private int jumps = 0;                      // Number of jumps currently available.
    #endregion

    // Use this for initialization
    void Start ()
    {
        #region Set up references
        rb = GetComponent<Rigidbody2D>();
        Transform groundTriggers = transform.Find("Ground_Triggers");
        groundCheck = groundTriggers.Find("Center").GetComponent<BoxCollider2D>();
        groundLeft = groundTriggers.Find("Left_Whisker").GetComponent<BoxCollider2D>();
        groundRight = groundTriggers.Find("Right_Whisker").GetComponent<BoxCollider2D>();
        groundMask = LayerMask.GetMask("Ground");
        #endregion

        #region Initialize variables
        #endregion
    }

    // Update is called once per frame
    void Update ()
    {
		
	}

    void FixedUpdate()
    {
        #region Physics Variables
        grounded = Physics2D.IsTouchingLayers(groundCheck, groundMask);
        groundedLeft = Physics2D.IsTouchingLayers(groundLeft, groundMask);
        groundedRight = Physics2D.IsTouchingLayers(groundRight, groundMask);
        #endregion

        #region State and Behavior Transitions
        if (grounded)
        {
            switch (state)
            {
                case ("standing"):
                    switch (behavior)
                    {
                        case ("moveLeft"):
                            if (groundedLeft)
                            {

                            }
                            else
                            {
                                if (Time.frameCount - behaviorWait > behaviorTime)
                                {
                                    rb.velocity = Vector2.zero;
                                    behavior = "moveRight";
                                    behaviorTime = Time.frameCount;
                                }
                            }
                            break;

                        case ("moveRight"):
                            if (groundedRight)
                            {

                            }
                            else
                            {
                                if (Time.frameCount - behaviorWait > behaviorTime)
                                {
                                    rb.velocity = Vector2.zero;
                                    behavior = "moveLeft";
                                    behaviorTime = Time.frameCount;
                                }
                            }
                            break;
                    }
                    break;
            }
        }
        else
        {
            switch (state)
            {
                case ("standing"):
                    switch (behavior)
                    {
                        case ("moveLeft"):
                            break;

                        case ("moveRight"):
                            break;
                    }
                    break;
            }
        }
        
        #endregion

        #region Movement
        float vx, vy;
        switch(state)
        {
            case ("standing"):
                switch (behavior)
                {
                    case ("moveLeft"):
                        if (Time.frameCount - behaviorWait > behaviorTime)
                        {
                            vx = rb.velocity.x;
                            // If the enemy's horizontal velocity is less than max speed add force to the enemy based on input.
                            if (Mathf.Abs(vx) < maxSpeed)
                                rb.AddForce(-Vector2.right * moveForce);

                            // If the enemy's horizontal velocity is greater than the maxSpeed set it equal to the maxSpeed.
                            if (Mathf.Abs(rb.velocity.x) > maxSpeed)
                                rb.velocity = new Vector2(Mathf.Sign(rb.velocity.x) * maxSpeed, rb.velocity.y);
                        }
                        break;

                    case ("moveRight"):
                        if (Time.frameCount - behaviorWait > behaviorTime)
                        {
                            vx = rb.velocity.x;
                            // If the enemy's horizontal velocity is less than max speed add force to the enemy based on input.
                            if (Mathf.Abs(vx) < maxSpeed)
                                rb.AddForce(Vector2.right * moveForce);

                            // If the enemy's horizontal velocity is greater than the maxSpeed set it equal to the maxSpeed.
                            if (Mathf.Abs(rb.velocity.x) > maxSpeed)
                                rb.velocity = new Vector2(Mathf.Sign(rb.velocity.x) * maxSpeed, rb.velocity.y);
                        }
                        break;
                }
                break;
        }
        #endregion
    }
}
