using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PixelPhysics : MonoBehaviour
{
    //TODO IMPORTANT Make sure objects are only added or removed before or after physics frames. Objects should only be marked for deletion within a frame.

    public float unitToPixel;
    private float pixelToUnit;

    public List<StaticObject> staticObjects = new List<StaticObject>();
    public List<DynamicObject> dynamicObjects = new List<DynamicObject>();
    
    private StaticObject nullStObj;
    private DynamicObject nullDObj;

    //public class StaticObject
    //{
    //    public int index;
    //    public string type;
    //    public Vector2 size;
    //    public Vector2 pos;
    //
    //    public StaticObject(int i, string t, Vector2 s, Vector2 p)
    //    {
    //        index = i;
    //        type = t;
    //        size = s;
    //        pos = p;
    //    }
    //}

    //public class StaticTri : StaticObject
    //{
    //    public float hypAng;
    //
    //    public StaticTri(int i, string t, float a, Vector2 s, Vector2 p) : base(i, t, s, p)
    //    {
    //        hypAng = a;
    //    }
    //}

    //public class DynamicObject
    //{
    //    public int index;
    //    public string type;
    //    public List<string> overlapTypes;
    //    public List<string> collisionTypes;
    //
    //    public float mass;
    //    public Vector2 size;
    //    public Vector2 pos;
    //    public Vector2 delta;
    //
    //    public List<StaticObject> staticOverlaps;
    //    public List<StaticObject> staticColsLeft;
    //    public List<StaticObject> staticColsRight;
    //    public List<StaticObject> staticColsDown;
    //    public List<StaticObject> staticColsUp;
    //
    //    public List<DynamicObject> dynamicOverlaps;
    //    public List<DynamicObject> dynamicColsLeft;
    //    public List<DynamicObject> dynamicColsRight;
    //    public List<DynamicObject> dynamicColsDown;
    //    public List<DynamicObject> dynamicColsUp;
    //
    //    public DynamicObject(int i, string t, List<string> o, List<string> c, float m, Vector2 s, Vector2 p, Vector2 d)
    //    {
    //        index = i;
    //        type = t;
    //        overlapTypes = o;
    //        collisionTypes = c;
    //
    //        mass = m;
    //        size = s;
    //        pos = p;
    //        delta = d;
    //
    //        staticOverlaps = new List<StaticObject>();
    //        staticColsLeft = new List<StaticObject>();
    //        staticColsRight = new List<StaticObject>();
    //        staticColsDown = new List<StaticObject>();
    //        staticColsUp = new List<StaticObject>();
    //
    //        dynamicOverlaps = new List<DynamicObject>();
    //        dynamicColsLeft = new List<DynamicObject>();
    //        dynamicColsRight = new List<DynamicObject>();
    //        dynamicColsDown = new List<DynamicObject>();
    //        dynamicColsUp = new List<DynamicObject>();
    //    }
    //
    //    public void moveX(float distance)
    //    {
    //        pos.x += distance;
    //        delta.Set(delta.x - distance, delta.y);
    //    }
    //
    //    public void moveY(float distance)
    //    {
    //        pos.y += distance;
    //        delta.Set(delta.x - distance, delta.y);
    //    }
    //}

    // Start is called before the first frame update
    void Start()
    {
        pixelToUnit = 1 / unitToPixel;
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void LateUpdate()
    {
        Setup();
        PixelMarch();
        Cleanup();
    }

    private void Setup()
    {
        for (int i = 0; i < dynamicObjects.Count; i++)
        {
            DynamicObject dObj = dynamicObjects[i];

            dObj.transform.position = dObj.pos - dObj.offset;
            dObj.vel += dObj.grav * Time.deltaTime;
            float newX = Mathf.Clamp(dObj.vel.x, -dObj.maxSpeedX, dObj.maxSpeedX);
            float newY = Mathf.Clamp(dObj.vel.y, -dObj.maxSpeedY, dObj.maxSpeedY);
            dObj.vel.Set(newX, newY);
            dObj.delta += dObj.vel * Time.deltaTime;
            dObj.collisions.Clear();

            dObj.projectedPos = dObj.pos + dObj.delta;
            float minX = Mathf.Min(dObj.pos.x, dObj.projectedPos.x) - (dObj.size.x / 2);
            float maxX = Mathf.Max(dObj.pos.x, dObj.projectedPos.x) + (dObj.size.x / 2);
            float minY = Mathf.Min(dObj.pos.y, dObj.projectedPos.y) - (dObj.size.y / 2);
            float maxY = Mathf.Max(dObj.pos.y, dObj.projectedPos.y) + (dObj.size.y / 2);
            Vector2 bl = new Vector2(minX, minY);
            Vector2 tr = new Vector2(maxX, maxY);
            dObj.nearbyStatic = checkBoxStatic(bl, tr, staticObjects);
        }
    }

    private void PixelMarch()
    {
        bool march = true;
        //Loop through all dynamic objects, finding the one with the largest x or y delta. Move that one in the specified direction and trigger a collision if necessary.
        while (march)
        {
            DynamicObject cur;
            Vector2 curDelta;
            float maxX = 0;
            float maxY = 0;
            int maxXI = 0;
            int maxYI = 0;
            for (int i = 0; i < dynamicObjects.Count; i++)
            {
                curDelta = dynamicObjects[i].delta;
                float absX = Mathf.Abs(curDelta.x);
                float absY = Mathf.Abs(curDelta.y);
                if (absX > maxX)
                {
                    maxX = absX;
                    maxXI = i;
                }
                if (absY > maxY)
                {
                    maxY = absY;
                    maxYI = i;
                }
            }

            if ((maxX <= pixelToUnit / 2) && (maxY <= pixelToUnit / 2))
            {
                march = false;
            }
            else if (maxX >= maxY)
            {
                cur = dynamicObjects[maxXI];
                if (cur.delta.x < 0)
                {
                    Vector2 bL = cur.pos + new Vector2(-(cur.size.x / 2f) - pixelToUnit, -cur.size.y / 2f);
                    Vector2 tR = cur.pos + new Vector2(-cur.size.x / 2f, cur.size.y / 2f);
                    foreach (StaticObject other in checkBoxStatic(bL, tR, cur.nearbyStatic))
                    {
                        staticCollision(cur, other, 180);
                    }
                    foreach (DynamicObject other in checkBoxDynamic(bL, tR, dynamicObjects))
                    {
                        if (cur.id != other.id)
                        {
                            dynamicCollision(cur, other, 180);
                        }
                    }
                }
                else if (cur.delta.x > 0)
                {
                    Vector2 bL = cur.pos + new Vector2(cur.size.x / 2f, -cur.size.y / 2f);
                    Vector2 tR = cur.pos + new Vector2((cur.size.x / 2f) + pixelToUnit, cur.size.y / 2f);
                    foreach (StaticObject other in checkBoxStatic(bL, tR, cur.nearbyStatic))
                    {
                        staticCollision(cur, other, 0);
                    }
                    foreach (DynamicObject other in checkBoxDynamic(bL, tR, dynamicObjects))
                    {
                        if (cur.id != other.id)
                        {
                            dynamicCollision(cur, other, 0);
                        }
                    }
                }

                if (cur.delta.x < -pixelToUnit / 2)
                {
                    cur.moveX(-pixelToUnit);
                }

                else if (cur.delta.x > pixelToUnit / 2)
                {
                    cur.moveX(pixelToUnit);
                }
            }
            else
            {
                cur = dynamicObjects[maxYI];
                if (cur.delta.y < 0)
                {
                    Vector2 bL = cur.pos + new Vector2(-cur.size.x / 2f, -(cur.size.y / 2f) - pixelToUnit);
                    Vector2 tR = cur.pos + new Vector2(cur.size.x / 2f, -(cur.size.y / 2f));
                    foreach (StaticObject other in checkBoxStatic(bL, tR, cur.nearbyStatic))
                    {
                        staticCollision(cur, other, 270);
                    }
                    foreach (DynamicObject other in checkBoxDynamic(bL, tR, dynamicObjects))
                    {
                        if (cur.id != other.id)
                        {
                            dynamicCollision(cur, other, 270);
                        }
                    }
                }
                else if (cur.delta.y > 0)
                {
                    Vector2 bL = cur.pos + new Vector2(-cur.size.x / 2f, cur.size.y / 2f);
                    Vector2 tR = cur.pos + new Vector2(cur.size.x / 2f, (cur.size.y / 2f) + pixelToUnit);
                    foreach (StaticObject other in checkBoxStatic(bL, tR, cur.nearbyStatic))
                    {
                        staticCollision(cur, other, 90);
                    }
                    foreach (DynamicObject other in checkBoxDynamic(bL, tR, dynamicObjects))
                    {
                        if (cur.id != other.id)
                        {
                            dynamicCollision(cur, other, 90);
                        }
                    }
                }

                if (cur.delta.y < -pixelToUnit / 2)
                {
                    cur.moveY(-pixelToUnit);
                }

                else if (cur.delta.y > pixelToUnit / 2)
                {
                    cur.moveY(pixelToUnit);
                }
            }
        }

        //Do one more set of collision checks for the leftover delta.
        foreach (DynamicObject cur in dynamicObjects)
        {
            if (cur.delta.x < 0)
            {
                Vector2 bL = cur.pos + new Vector2(-(cur.size.x / 2f) - pixelToUnit, -cur.size.y / 2f);
                Vector2 tR = cur.pos + new Vector2(-cur.size.x / 2f, cur.size.y / 2f);
                foreach (StaticObject other in checkBoxStatic(bL, tR, cur.nearbyStatic))
                {
                    staticCollision(cur, other, 180);
                }
                foreach (DynamicObject other in checkBoxDynamic(bL, tR, dynamicObjects))
                {
                    if (cur.id != other.id)
                    {
                        dynamicCollision(cur, other, 180);
                    }
                }
            }
            else if (cur.delta.x > 0)
            {
                Vector2 bL = cur.pos + new Vector2(cur.size.x / 2f, -cur.size.y / 2f);
                Vector2 tR = cur.pos + new Vector2((cur.size.x / 2f) + pixelToUnit, cur.size.y / 2f);
                foreach (StaticObject other in checkBoxStatic(bL, tR, cur.nearbyStatic))
                {
                    staticCollision(cur, other, 0);
                }
                foreach (DynamicObject other in checkBoxDynamic(bL, tR, dynamicObjects))
                {
                    if (cur.id != other.id)
                    {
                        dynamicCollision(cur, other, 0);
                    }
                }
            }
        
            if (cur.delta.y < 0)
            {
                Vector2 bL = cur.pos + new Vector2(-cur.size.x / 2f, -(cur.size.y / 2f) - pixelToUnit);
                Vector2 tR = cur.pos + new Vector2(cur.size.x / 2f, -(cur.size.y / 2f));
                foreach (StaticObject other in checkBoxStatic(bL, tR, cur.nearbyStatic))
                {
                    staticCollision(cur, other, 270);
                }
                foreach (DynamicObject other in checkBoxDynamic(bL, tR, dynamicObjects))
                {
                    if (cur.id != other.id)
                    {
                        dynamicCollision(cur, other, 270);
                    }
                }
            }
            else if (cur.delta.y > 0)
            {
                Vector2 bL = cur.pos + new Vector2(-cur.size.x / 2f, cur.size.y / 2f);
                Vector2 tR = cur.pos + new Vector2(cur.size.x / 2f, (cur.size.y / 2f) + pixelToUnit);
                foreach (StaticObject other in checkBoxStatic(bL, tR, cur.nearbyStatic))
                {
                    staticCollision(cur, other, 90);
                }
                foreach (DynamicObject other in checkBoxDynamic(bL, tR, dynamicObjects))
                {
                    if (cur.id != other.id)
                    {
                        dynamicCollision(cur, other, 90);
                    }
                }
            }
        }
    }

    private void Cleanup()
    {

    }


    public int registerStatic(StaticObject stObj)
    {
        staticObjects.Add(stObj);
        return dynamicObjects.Count;
    }

    public int registerDynamic(DynamicObject dObj)
    {
        dynamicObjects.Add(dObj);
        return dynamicObjects.Count;
    }


    #region Overlaps and collisions
    public List<StaticObject> checkBoxStatic(Vector2 bottomLeft, Vector2 topRight, List<StaticObject> stObjs)               //Returns all elements of stObjs overlapping the box.
    {
        List<StaticObject> ret = new List<StaticObject>();

        for(int i = 0; i < stObjs.Count; i++)
        {
            StaticObject other = stObjs[i];
            Vector2 otherBL = other.pos - other.size / 2f;
            Vector2 otherTR = other.pos + other.size / 2f;
            if (bottomLeft.x >= otherTR.x || bottomLeft.y >= otherTR.y || topRight.x <= otherBL.x || topRight.y <= otherBL.y)
            {
                continue;
            }
            ret.Add(stObjs[i]);
        }
        return ret;
    }

    public List<DynamicObject> checkBoxDynamic(Vector2 bottomLeft, Vector2 topRight, List<DynamicObject> dynObjs)             //Returns all type t ShysicsObjects overlapping the box.
    {
        List<DynamicObject> ret = new List<DynamicObject>();

        for(int i = 0; i < dynObjs.Count; i++)
        {
            DynamicObject other = dynamicObjects[i];
            Vector2 otherBL = other.pos - other.size / 2f;
            Vector2 otherTR = other.pos + other.size / 2f;
            if (bottomLeft.x > otherTR.x || bottomLeft.y > otherTR.y || topRight.x < otherBL.x || topRight.y < otherBL.y)
            {
                continue;
            }
            ret.Add(dynObjs[i]);
        }
        return ret;
    }

    private void staticCollision(DynamicObject dObj, StaticObject stObj, float angle)
    {
        float speed = dObj.vel.magnitude;
        float disp = dObj.delta.magnitude;

        float velAngle = Vector2.SignedAngle(Vector2.right, dObj.vel);
        float impactAngle = (velAngle - angle + 360f) % 360f;
        //float parallelAngle = (angle + 450f) % 360f;

        float normalFactor = -Mathf.Cos(impactAngle * Mathf.Rad2Deg);
        float parallelFactor = Mathf.Sin(impactAngle * Mathf.Deg2Rad);
        float frictionFactor = Mathf.Abs(normalFactor * (dObj.friction + stObj.friction));
        float bounceFactor = normalFactor * (dObj.elasticity + stObj.elasticity) / 2;

        float parallelSpeed;
        float parallelDisp;
        if (Mathf.Abs(parallelFactor) <= frictionFactor)
        {
            parallelSpeed = 0;
            parallelDisp = 0;
        }
        else
        {
            parallelSpeed = (speed * parallelFactor) - (speed * frictionFactor * Mathf.Sign(parallelFactor));
            parallelDisp = (disp * parallelFactor) - (disp * frictionFactor * Mathf.Sign(parallelFactor));
        }

        float normalSpeed = speed * bounceFactor;
        float normalDisp = disp * bounceFactor;

        float sinA = Mathf.Sin(angle * Mathf.Deg2Rad);
        float cosA = Mathf.Cos(angle * Mathf.Deg2Rad);
        //float sinP = Mathf.Sin(parallelAngle * Mathf.Deg2Rad);
        //float cosP = Mathf.Cos(parallelAngle * Mathf.Deg2Rad);

        float newVelX = parallelSpeed * -sinA + normalSpeed * cosA;
        float newVelY = parallelSpeed * cosA + normalSpeed * sinA;
        float newDeltaX = parallelDisp * -sinA + normalDisp * cosA;
        float newDeltaY = parallelDisp * cosA + normalDisp * sinA;

        dObj.vel = new Vector2(newVelX, newVelY);
        dObj.delta = new Vector2(newDeltaX, newDeltaY);

        //dObj.vel = new Vector2(parallelSpeed * Mathf.Cos((parallelAngle) * Mathf.Deg2Rad), parallelSpeed * Mathf.Sin((parallelAngle) * Mathf.Deg2Rad));
        //dObj.delta = new Vector2(parallelDisp * Mathf.Cos((parallelAngle) * Mathf.Deg2Rad), parallelDisp * Mathf.Sin((parallelAngle) * Mathf.Deg2Rad));
        dObj.registerCol(false, stObj.id, stObj.type, dObj.pos, angle);
    }

    private void dynamicCollision(DynamicObject dObjA, DynamicObject dObjB, float angle)
    {
        Vector2 impactUnit = RadianToVector2(angle);
        Vector2 impactMomentum = dObjA.mass * dObjA.vel * impactUnit + dObjB.mass * dObjB.vel * -impactUnit;

        dObjA.vel = dObjA.vel - dObjA.vel * impactUnit + impactMomentum / dObjA.mass;
        dObjB.vel = dObjB.vel - dObjB.vel * -impactUnit + impactMomentum / dObjB.mass;

        dObjA.delta -= dObjA.delta * impactUnit;
        dObjB.delta -= dObjB.delta * -impactUnit;

        dObjA.registerCol(false, dObjB.id, dObjB.type, dObjA.pos, angle);
        dObjB.registerCol(false, dObjA.id, dObjA.type, dObjB.pos, (angle + 180) % 360);
    }
    #endregion



    private Vector2 pixelClamp(Vector2 v)
    {
        Vector2 pixelVector = new Vector2(Mathf.RoundToInt(v.x * unitToPixel), Mathf.RoundToInt(v.y * unitToPixel));
        return pixelVector * pixelToUnit;
    }
    public static Vector2 RadianToVector2(float radian)
    {
        return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));
    }

    public static Vector2 DegreeToVector2(float degree)
    {
        return RadianToVector2(degree * Mathf.Deg2Rad);
    }
}