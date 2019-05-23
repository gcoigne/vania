using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_Follow : MonoBehaviour {

    public float xMargin = 1f;          // Distance in the x axis the target can move before the camera follows.
    public float yMargin = 1f;          // Distance in the y axis the target can move before the camera follows.
    public float maxSpeed = 20f;        // Maximum speed for the camera to move.
    public float smoothTime = 8f;       // Number of frames for the camera to reach the target location.
    public float leadTime = 0f;         // If nonzero, the camera predicts the target's position in leadTime seconds based on current rb.velocity.
    public float maxX;                  // Bounding coordinates for the camera's location.
    public float maxY;              
    public float minX;              
    public float minY;

    private Vector3 targetVector;       // The location to move toward.
    private Vector3 destVector;         // The actual location to move to.
    private GameObject target;          // The GameObject to follow.
    private Transform targetT;          // The target's Transform.
    private Rigidbody2D targetRB;       // The target's Rigidbody2D
    private Vector3 panVelocity;        // Current camera velocity.

    // Use this for initialization
    void Start ()
    {
        // Attempt to target the player.
        findTarget("Player");

        panVelocity = Vector3.zero;
	}
	
	// Update is called once per frame
	void Update ()
    {
        trackTarget();
    }

    private void FixedUpdate()
    {

    }

    void trackTarget()
    {
        // By default the destination vector is the current position.
        destVector = transform.position;

        // Attempt to target the player
        if (targetT == null)
        {
            findTarget("Player");
        }

        if (leadTime > 0)
        {
            // The target vector is where the player would arrive after leadTime seconds with constant velocity.
            Vector2 targetVel = targetRB.velocity;
            float targetX = Mathf.Clamp(targetT.position.x + targetVel.x * leadTime, targetT.position.x - xMargin, targetT.position.x + xMargin);
            float targetY = Mathf.Clamp(targetT.position.y + targetVel.y * leadTime, targetT.position.y - yMargin, targetT.position.y + yMargin);
            targetVector = new Vector3(targetX, targetY, transform.position.z);
        }
        else
        {
            targetVector = new Vector3(targetT.position.x, targetT.position.y, transform.position.z);
        }

        // If the target position is outside the margins
        if (Mathf.Abs(transform.position.x - targetVector.x) > xMargin || Mathf.Abs(transform.position.y - targetVector.y) > yMargin)
        {
            // Move toward the target position.
            destVector = Vector3.SmoothDamp(transform.position, targetVector, ref panVelocity, smoothTime, maxSpeed);
        }

        // Clamp x and y
        destVector.x = Mathf.Clamp(destVector.x, minX, maxX);
        destVector.y = Mathf.Clamp(destVector.y, minY, maxY);
        //destVector.x = Mathf.Clamp(destVector.x, targetT.position.x - xMargin, targetT.position.x + xMargin);
        //destVector.y = Mathf.Clamp(destVector.y, targetT.position.y - yMargin, targetT.position.y + xMargin);

        // Move the camera.
        transform.position = destVector;
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

    // Returns true if the distance between the camera and the target in the x axis is greater than the x margin.
    private bool checkXMargin()
    {
        return Mathf.Abs(transform.position.x - targetT.position.x) > xMargin;
    }

    // Returns true if the distance between the camera and the target in the y axis is greater than the y margin.
    private bool checkYMargin()
    {
        return Mathf.Abs(transform.position.y - targetT.position.y) > yMargin;
    }
}
