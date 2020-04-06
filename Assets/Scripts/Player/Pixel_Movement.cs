using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pixel_Movement : MonoBehaviour
{
    public int ppu = 16;
    public float width;
    public float height;

    public float grav = 10f;
    public float mass = 1f;
    public float moveForce = 50;
    public float maxSpeedX = 10;
    public float maxSpeedY = 20;

    private float upp;
    private float borderX;
    private float borderY;

    private float hInput;
    private float vInput;
    private bool jumpPress;
    private bool jumpHold;

    private bool grounded;

    private Vector2 vel;
    private Vector2 updateVel;          //Velocity adjusted for deltaTime
    private Vector2 target;             //Vector to move
    private Vector2 remVel;             //Player always moves to the pixel nearest their velocity. Remainder is stored for the next frame.

    private LayerMask groundMask;
    private LayerMask smoothMask;
    private LayerMask roughMask;
    private ContactFilter2D surfaceFilter;

    private void Start()
    {
        upp = 1f / ppu;
        borderX = width / 2;
        borderY = height / 2;

        vel = new Vector2(0f, 0f);
        updateVel = vel;
        target = transform.position;

        groundMask = LayerMask.GetMask("Ground");
        smoothMask = LayerMask.GetMask("Smooth");
        roughMask = LayerMask.GetMask("Rough");
        surfaceFilter = new ContactFilter2D();
    }
    private void Update()
    {
        hInput = Input.GetAxis("Horizontal");
        vInput = Input.GetAxis("Vertical");
        jumpPress = Input.GetButtonDown("Jump");
        jumpHold = Input.GetButton("Jump");

        grounded = !checkY(transform.position.y - borderY);

        float tempVelX = vel.x;
        float tempVelY = vel.y;

        if (grounded)
        {
            tempVelX += hInput * moveForce * Time.deltaTime / mass;
            tempVelY = Mathf.Max(tempVelY, 0);
        }
        else
        {
            tempVelY -= grav * Time.deltaTime;
        }

        #region clamp x and y speed
        if (tempVelX < 0)
        {
            tempVelX = Mathf.Max(tempVelX, -maxSpeedX);
        }
        else
        {

            tempVelX = Mathf.Min(tempVelX, maxSpeedX);
        }

        if (tempVelY < 0)
        {
            tempVelY = Mathf.Max(tempVelY, -maxSpeedY);
        }
        else
        {

            tempVelY = Mathf.Min(tempVelY, maxSpeedY);
        }
        #endregion

        vel = new Vector2(tempVelX, tempVelY);

        updateVel = (vel) * Time.deltaTime;
        //updateVel = (vel + remVel) * Time.deltaTime;

        //transform.Translate((Vector3) updateVel);
        pixelMarch(updateVel);
    }

    private void FixedUpdate()
    {
    }

    private bool checkX(float offset)
    {
        Collider2D col;
        col = Physics2D.OverlapBox(new Vector2(offset, 0), new Vector2(upp, height), 0, groundMask);
        if (col != null)
        {
            return false;
        }
        col = Physics2D.OverlapBox(new Vector2(offset, 0), new Vector2(upp, height), 0, smoothMask);
        if (col != null)
        {
            return false;
        }
        col = Physics2D.OverlapBox(new Vector2(offset, 0), new Vector2(upp, height), 0, roughMask);
        if (col != null)
        {
            return false;
        }
        return true;
    }

    private bool checkY(float offset)
    {
        Collider2D col;
        col = Physics2D.OverlapBox(new Vector2(0, offset), new Vector2(width, upp), 0, groundMask);
        if (col != null)
        {
            return false;
        }
        col = Physics2D.OverlapBox(new Vector2(0, offset), new Vector2(width, upp), 0, smoothMask);
        if (col != null)
        {
            return false;
        }
        col = Physics2D.OverlapBox(new Vector2(0, offset), new Vector2(width, upp), 0, roughMask);
        if (col != null)
        {
            return false;
        }
        return true;
    }

    private bool checkPos(Vector2 offset)           //Returns true if the object can move one pixel in that direction.
    {
        Vector2 size = new Vector2(width, height);
        Collider2D col;
        col = Physics2D.OverlapBox(offset, size, 0, groundMask);
        if (col != null)
        {
            return false;
        }
        col = Physics2D.OverlapBox(offset, size, 0, smoothMask);
        if (col != null)
        {
            return false;
        }
        col = Physics2D.OverlapBox(offset, size, 0, roughMask);
        if (col != null)
        {
            return false;
        }
        return true;
    }

    private void pixelMarch(Vector2 vel)            //Moves the player pixel by pixel toward vel, stopping on collision. Returns remainder of vel.
    {
        Vector2 tempVel = vel;
        float tempVelX = tempVel.x;
        float tempVelY = tempVel.y;

        float targetX = transform.position.x;
        float targetY = transform.position.y;

        while (Mathf.Abs(tempVelX) > upp/2 || Mathf.Abs(tempVelY) > upp/2)
        {
            if (Mathf.Abs(tempVelX) >= Mathf.Abs(tempVelY))
            {
                if (tempVelX < 0)
                {
                    if (checkX(targetX - borderX))
                    {
                        tempVelX += upp;
                        targetX -= upp;
                    }
                    else
                    {
                        //remVelX = tempVelX;
                        tempVelX = 0;
                    }
                }
                else
                {
                    if (checkX(targetX + borderX))
                    {
                        tempVelX -= upp;
                        targetX += upp;
                    }
                    else
                    {
                        //remVelX = tempVelX;
                        tempVelX = 0;
                    }
                }
            }
            else
            {
                if (tempVelY < 0)
                {
                    if (checkY(targetY - borderY))
                    {
                        tempVelY += upp;
                        targetY -= upp;
                    }
                    else
                    {
                        //remVelY = tempVelY;
                        tempVelY = 0;
                    }
                }
                else
                {
                    if (checkY(targetY + borderY))
                    {
                        tempVelY -= upp;
                        targetY += upp;
                    }
                    else
                    {
                        //remVelY = tempVelY;
                        tempVelY = 0;
                    }
                }
            }
        }

        target = new Vector2(targetX, targetY);
        transform.position = target;

        remVel = new Vector2(tempVelX, tempVelY) / Time.deltaTime;
    }

    private Vector2 pixelClamp(Vector2 v)
    {
        Vector2 pixelVector = new Vector2(Mathf.RoundToInt(v.x * ppu), Mathf.RoundToInt(v.y * ppu));
        return pixelVector / ppu;
    }

    private float getAngle(Vector2 v)
    {
        return Vector2.Angle(Vector2.right, v);
    }
}
