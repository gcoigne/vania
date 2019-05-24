using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_Dynamic : MonoBehaviour {

    #region Objects and components
    private Rigidbody2D rb;                 // The camera's Rigidbody2D
    private GameObject target;              // The GameObject to follow
    private Transform targetT;              // The target's Transform
    private Rigidbody2D targetRB;           // The target's Rigidbody2D
    private Vector2 targetV;                // The location to move toward
    #endregion

    #region Variables
    public float maxSpeed = 20f;            // Maximum speed
    public float maxForce = 20f;            // Maximum force for camera acceleration
    public float relForceX = 1f;            // Amount of force to apply relative to target velocity.
    public float relForceY = 1f;            //
    private Vector2 forceV = Vector2.zero;  // Current force applied to the camera
    public float leadTime = 1f;             // Number of seconds in advance the camera predicts target movement
    public float marginX = 1f;              // Minimum distance to targetV for the camera to accelerate. Margin circumference is an oval with x,y radii marginX, marginY
    public float marginY = 1f;              //
    public float leashX = 4f;               // Maximum distance the camera can float away from the player before getting pulled back in. Oval circumference
    public float leashY = 4f;               //
    #endregion

    // Use this for initialization
    void Start ()
    {
        rb = GetComponent<Rigidbody2D>();
	}
	
	// Update is called once per frame
	void Update ()
    {
    
	}

    private void FixedUpdate()
    {
        trackTarget();
    }

    void trackTarget()
    {
        // Attempt to target the player
        if (targetT == null)
        {
            if (!findTarget("Player")) return;
        }
        Vector2 selfP = transform.position;
        Vector2 targetP = targetT.position;

        if (leadTime > 0)
        {
            // The target vector is where the player would arrive after leadTime seconds with constant velocity.
            Vector2 targetVel = targetRB.velocity;
            targetV = new Vector2(targetP.x + targetVel.x * leadTime, targetP.y + targetVel.y * leadTime);
        }
        else
        {
            targetV = new Vector2(targetP.x, targetP.y);
        }

        // If the target position is between the margins
        if (withinEllipse(targetV, targetP, leashX, leashY) && !withinEllipse(targetV, targetP, marginX, marginY))
        {
            forceV = Vector2.ClampMagnitude(new Vector2((targetV.x - selfP.x) * relForceX, (targetV.y - selfP.y) * relForceY), maxForce);
        }
        else
        {
            forceV = Vector2.zero;
        }
        rb.AddForce(forceV);
        rb.velocity = Vector2.ClampMagnitude(rb.velocity, maxSpeed);
    }

    // Attempts to find a GameObject with the given name. Modifies target, targetT, and TargetRB if successful.
    private bool findTarget(string targetName)
    {
        target = GameObject.Find(targetName);
        if (target)
        {
            targetT = target.transform;
            targetRB = target.GetComponent<Rigidbody2D>();
            return true;
        }
        return false;
    }

    private bool withinEllipse(Vector2 testV, Vector2 origin, float rx, float ry)
    {
        return Mathf.Pow(testV.x - origin.x, 2f) / Mathf.Pow(rx, 2f) + Mathf.Pow(testV.y - origin.y, 2f) / Mathf.Pow(ry, 2f) < 1;
    }
}
