using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour {

    #region References to objects
    private Rigidbody2D rb;                     // Reference to the enemy's physics object.
    private BoxCollider2D groundCheck;          // A hitbox that checks if the enemy is grounded.
    private Animator anim;                      // Reference to the enemy's animator component.
    private LayerMask groundMask;               // Reference to Ground layer mask.
    #endregion

    #region Physics variables
    private Vector2[] groundPoints;
    public bool grounded = false;               // Whether or not the enemy is in contact with the ground.
    #endregion

    #region State and Behavior Variables
    public string state = "standing";
    public string behavior = "none";
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
        groundCheck = transform.Find("Ground_Check").GetComponent<BoxCollider2D>();
        //anim = GetComponent<Animator>();
        groundMask = LayerMask.GetMask("Ground");
        #endregion

        #region Initialize variables
        groundPoints[0] = new Vector2(transform.position.x - transform.localScale.x / 2, transform.position.y - transform.localScale.y / 2);
        groundPoints[1] = new Vector2(transform.position.x, transform.position.y - transform.localScale.y / 2);
        groundPoints[2] = new Vector3(transform.position.x + transform.localScale.x / 2, transform.position.y - transform.localScale.y / 2);
        #endregion
    }

    // Update is called once per frame
    void Update ()
    {
		
	}

    void FixedUpdate()
    {
        #region movement
        switch(state)
        {
            case ("standing"):
                switch (behavior)
                {
                    case ("moveLeft"):
                        float vx = rb.velocity.x;
                        // If the enemy's horizontal velocity is less than max speed add force to the enemy based on input.
                        if (-vx < maxSpeed)
                            rb.AddForce(-Vector2.right * moveForce);

                        // If the enemy's horizontal velocity is greater than the maxSpeed set it equal to the maxSpeed.
                        if (Mathf.Abs(rb.velocity.x) > maxSpeed)
                            rb.velocity = new Vector2(Mathf.Sign(rb.velocity.x) * maxSpeed, rb.velocity.y);
                        break;
                }
                break;
        }
        #endregion
    }
}
