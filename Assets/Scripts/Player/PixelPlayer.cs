using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PixelPlayer : MonoBehaviour
{
    private DynamicObject dObj;
    private Scanner downScanner;

    public float moveForce;
    public float jumpForce;

    public List<string> overlapTypes;
    public List<string> collisionTypes;

    private float hInput;
    private float vInput;
    private bool jumpPress;
    private bool jumpHold;

    public bool grounded;

    private void Start()
    {
        dObj = GetComponent<DynamicObject>();
        downScanner = transform.Find("DownScanner").GetComponent<Scanner>();
        dObj.overlapTypes = overlapTypes;
        dObj.collisionTypes = collisionTypes;
    }
    private void Update()
    {
        hInput = Input.GetAxis("Horizontal");
        vInput = Input.GetAxis("Vertical");
        jumpPress = Input.GetButtonDown("Jump");
        jumpHold = Input.GetButton("Jump");

        grounded = downScanner.checkStatic("ground").Count > 0;

        if (grounded)
        {
            if (jumpPress)
            {
                dObj.applyForce(new Vector2(0, jumpForce));
            }
            else if (Mathf.Abs(hInput) > 0)
            {
                dObj.applyForce(new Vector2(hInput * moveForce, 0) * Time.deltaTime);
            }
        }
    }
}