using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PixelPhysics : MonoBehaviour
{
    public float ppu;
    private float upp;
    public int buf = 64;

    public GameObject[] staticGameObjects;
    public GameObject[] dynamicGameObjects;
    public StaticObject[] staticObjects;
    public DynamicObject[] dynamicObjects;
    
    private StaticObject nullStObj;
    private DynamicObject nullDObj;

    public class StaticObject
    {
        public int buf;
        public float upp;
        public float ppu;

        public int index;
        public string type;

        public Vector2 size;
        public Vector2 pos;

        public StaticObject(int i, string t, Vector2 s, Vector2 p)
        {
            index = i;
            type = t;
            size = s;
            pos = p;
        }
    }

    public class DynamicObject
    {
        public int buf;

        public int index;
        public string type;
        public string[] overlapTypes;
        public string[] collisionTypes;

        public Vector2 size;
        public Vector2 pos;
        public Vector2 delta;

        public StaticObject[] overlapsStatic;
        public StaticObject[] colsLeftStatic;
        public StaticObject[] colsRightStatic;
        public StaticObject[] colsDownStatic;
        public StaticObject[] colsUpStatic;

        public DynamicObject[] overlapsDynamic;
        public DynamicObject[] colsLeftDynamic;
        public DynamicObject[] colsRightDynamic;
        public DynamicObject[] colsDownDynamic;
        public DynamicObject[] colsUpDynamic;

        public DynamicObject(int i, string t, string[] o, string[] c, Vector2 s, Vector2 p, Vector2 d)
        {
            buf = 64;

            index = i;
            type = t;
            overlapTypes = o;
            collisionTypes = c;
            size = s;
            pos = p;
            delta = d;

            overlapsStatic = new StaticObject[buf];
            colsLeftStatic = new StaticObject[buf];
            colsRightStatic = new StaticObject[buf];
            colsDownStatic = new StaticObject[buf];
            colsUpStatic = new StaticObject[buf];

            overlapsDynamic = new DynamicObject[buf];
            colsLeftDynamic = new DynamicObject[buf];
            colsRightDynamic = new DynamicObject[buf];
            colsDownDynamic = new DynamicObject[buf];
            colsUpDynamic = new DynamicObject[buf];
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        upp = 1 / ppu;

        staticObjects = new StaticObject[buf];
        int i = 0;
        foreach (GameObject gObj in staticGameObjects)
        {
            if (gObj == null)
            {
                break;
            }

            PixelStatic pixStat = gObj.GetComponent<PixelStatic>();
            string t = pixStat.type;
            Vector2 s = pixStat.size;
            Vector2 p = gObj.transform.position;

            StaticObject stObj = new StaticObject(i, t, s, p);
            staticObjects[i] = stObj;
        }

        dynamicObjects = new DynamicObject[buf];
        i = 0;
        foreach (GameObject gObj in dynamicGameObjects)
        {
            if (gObj == null)
            {
                break;
            }

            PixelMovement pixMov = gObj.GetComponent<PixelMovement>();
            string t = pixMov.type;
            string[] o = pixMov.overlapTypes;
            string[] c = pixMov.collisionTypes;
            Vector2 s = pixMov.size;
            Vector2 p = gObj.transform.position;
            Vector2 d = pixMov.movement;

            DynamicObject dObj = new DynamicObject(i, t, o, c, s, p, d);
            dynamicObjects[i] = dObj;
        }

        nullStObj = new StaticObject(-1, "null", Vector2.zero, Vector2.zero);
        nullDObj = new DynamicObject(-1, "null", new string[buf], new string[buf], Vector2.zero, Vector2.zero, Vector2.zero);
    }

    // Update is called once per frame
    void Update()
    {
        Vector2[] deltas = new Vector2[buf];
        DynamicObject cur;

        for (int i = 0; i < buf; i++)
        {
            if (dynamicObjects[i] is null)
            {
                break;
            }
            deltas[i] = dynamicObjects[i].delta;
        }

        float maxX = 0;
        float maxY = 0;
        int maxXI = 0;
        int maxYI = 0;
        for (int i = 0; i < buf; i++)
        {
            float absX = Mathf.Abs(deltas[i].x);
            float absY = Mathf.Abs(deltas[i].y);
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

        if (maxX > maxY)
        {
            cur = dynamicObjects[maxXI];
            float dx = deltas[maxXI].x;
            if (dx < 0)
            {
                cur.colsLeftStatic = checkColsLeftStatic(cur);
                cur.colsLeftDynamic = checkColsLeftDynamic(cur);

                if (cur.colsLeftStatic[0] != null && cur.colsLeftDynamic[0] != null);
            }
            else if (dx > 0)
            {
                cur.colsRightStatic = checkColsRightStatic(cur);
                cur.colsRightDynamic = checkColsRightDynamic(cur);
            }
        }
        else
        {
            cur = dynamicObjects[maxYI];
            float dy = deltas[maxYI].x;
            if (dy < 0)
            {
                cur.colsDownStatic = checkColsDownStatic(cur);
                cur.colsDownDynamic = checkColsDownDynamic(cur);
            }
            else if (dy > 0)
            {
                cur.colsUpStatic = checkColsUpStatic(cur);
                cur.colsUpDynamic = checkColsUpDynamic(cur);
            }
        }
    }

    private StaticObject[] checkBoxStatic(string t, Vector2 s, Vector2 p)               //Returns all type t StaticObjects overlapping the box.
    {
        StaticObject[] ret = new StaticObject[buf];
        int i = 0;

        foreach (StaticObject stObj in staticObjects)
        {
            if (stObj.type == t)
            {
                Vector2 stObjS = stObj.size;
                Vector2 stObjP = stObj.pos;
                if ((p.x - s.x) > (stObjP.x + stObjS.x) || (p.x + s.x) < (stObjP.x - stObjS.x) || (p.y - s.y) > (stObjP.y + stObjS.y) || (p.y + s.y) < (stObjP.y - stObjS.y))
                {
                    continue;
                }
                ret[i] = stObj;
                i++;
            }
        }
        return ret;
    }

    private DynamicObject[] checkBoxDynamic(string t, Vector2 s, Vector2 p)             //Returns all type t ShysicsObjects overlapping the box.
    {
        DynamicObject[] ret = new DynamicObject[buf];
        int i = 0;

        foreach (DynamicObject dObj in dynamicObjects)
        {
            if (dObj.type == t)
            {
                Vector2 dObjS = dObj.size;
                Vector2 dObjP = dObj.pos;
                if ((p.x - s.x) > (dObjP.x + dObjS.x) || (p.x + s.x) < (dObjP.x - dObjS.x) || (p.y - s.y) > (dObjP.y + dObjS.y) || (p.y + s.y) < (dObjP.y - dObjS.y))
                {
                    continue;
                }
                ret[i] = dObj;
                i++;
            }
        }
        return ret;
    }

    private StaticObject[] checkOverlapStatic(DynamicObject dObj)                      //Returns all matching StaticObjects overlapping dObj.
    {
        StaticObject[] overlaps = new StaticObject[buf];

        Vector2 s = dObj.size;
        Vector2 p = dObj.pos;

        int i = 0;
        foreach (string oT in dObj.overlapTypes)
        {
            foreach (StaticObject other in checkBoxStatic(oT, s, p))
            {
                overlaps[i] = other;
                i++;
            }
        }
        //dObj.overlapsStatic = overlaps;
        return overlaps;
    }

    private DynamicObject[] checkOverlapDynamic(DynamicObject dObj)                    //Returns all matching DynamicObjects overlapping dObj.
    {
        DynamicObject[] ret = new DynamicObject[buf];

        Vector2 s = dObj.size;
        Vector2 p = dObj.pos;

        int i = 0;
        foreach (string oT in dObj.overlapTypes)
        {
            foreach (DynamicObject other in checkBoxDynamic(oT, s, p))
            {
                ret[i] = other;
                i++;
            }
        }
        return ret;
    }

    private StaticObject[] checkColsLeftStatic(DynamicObject dObj)                      //Returns all matching StaticObjects one pixel to the left of dObj
    {

        StaticObject[] ret = new StaticObject[buf];

        Vector2 s = new Vector2(upp, dObj.size.y);
        Vector2 p = new Vector2(dObj.pos.x - (dObj.size.x / 2f), dObj.pos.y);

        int i = 0;
        foreach (string cT in dObj.collisionTypes)
        {
            foreach (StaticObject other in checkBoxStatic(cT, s, p))
            {
                ret[i] = other;
                i++;
            }
        }
        return ret;
    }               

    private StaticObject[] checkColsRightStatic(DynamicObject dObj)
    {

        StaticObject[] ret = new StaticObject[buf];

        string t = dObj.type;
        Vector2 s = new Vector2(upp, dObj.size.y);
        Vector2 p = new Vector2(dObj.pos.x + (dObj.size.x / 2f), dObj.pos.y);

        int i = 0;
        foreach (string cT in dObj.collisionTypes)
        {
            foreach (StaticObject other in checkBoxStatic(cT, s, p))
            {
                ret[i] = other;
                i++;
            }
        }
        return ret;
    }

    private StaticObject[] checkColsDownStatic(DynamicObject dObj)
    {

        StaticObject[] ret = new StaticObject[buf];

        string t = dObj.type;
        Vector2 s = new Vector2(dObj.size.x, upp);
        Vector2 p = new Vector2(dObj.pos.x, dObj.pos.y - (dObj.size.y / 2f));

        int i = 0;
        foreach (string cT in dObj.collisionTypes)
        {
            foreach (StaticObject other in checkBoxStatic(cT, s, p))
            {
                ret[i] = other;
                i++;
            }
        }
        return ret;
    }

    private StaticObject[] checkColsUpStatic(DynamicObject dObj)
    {

        StaticObject[] ret = new StaticObject[buf];

        string t = dObj.type;
        Vector2 s = new Vector2(dObj.size.x, upp);
        Vector2 p = new Vector2(dObj.pos.x, dObj.pos.y + (dObj.size.y / 2f));

        int i = 0;
        foreach (string cT in dObj.collisionTypes)
        {
            foreach (StaticObject other in checkBoxStatic(cT, s, p))
            {
                ret[i] = other;
                i++;
            }
        }
        return ret;
    }

    private DynamicObject[] checkColsLeftDynamic(DynamicObject dObj)                    //Returns all matching DynamicObjects one pixel to the left of dObj
    {

        DynamicObject[] ret = new DynamicObject[buf];

        Vector2 s = new Vector2(upp, dObj.size.y);
        Vector2 p = new Vector2(dObj.pos.x - (dObj.size.x / 2f), dObj.pos.y);

        int i = 0;
        foreach (string cT in dObj.collisionTypes)
        {
            foreach (DynamicObject other in checkBoxDynamic(cT, s, p))
            {
                ret[i] = other;
                i++;
            }
        }
        return ret;
    }

    private DynamicObject[] checkColsRightDynamic(DynamicObject dObj)
    {

        DynamicObject[] ret = new DynamicObject[buf];

        string t = dObj.type;
        Vector2 s = new Vector2(upp, dObj.size.y);
        Vector2 p = new Vector2(dObj.pos.x + (dObj.size.x / 2f), dObj.pos.y);

        int i = 0;
        foreach (string cT in dObj.collisionTypes)
        {
            foreach (DynamicObject other in checkBoxDynamic(cT, s, p))
            {
                ret[i] = other;
                i++;
            }
        }
        return ret;
    }

    private DynamicObject[] checkColsDownDynamic(DynamicObject dObj)
    {

        DynamicObject[] ret = new DynamicObject[buf];

        string t = dObj.type;
        Vector2 s = new Vector2(dObj.size.x, upp);
        Vector2 p = new Vector2(dObj.pos.x, dObj.pos.y - (dObj.size.y / 2f));

        int i = 0;
        foreach (string cT in dObj.collisionTypes)
        {
            foreach (DynamicObject other in checkBoxDynamic(cT, s, p))
            {
                ret[i] = other;
                i++;
            }
        }
        return ret;
    }

    private DynamicObject[] checkColsUpDynamic(DynamicObject dObj)
    {

        DynamicObject[] ret = new DynamicObject[buf];

        string t = dObj.type;
        Vector2 s = new Vector2(dObj.size.x, upp);
        Vector2 p = new Vector2(dObj.pos.x, dObj.pos.y + (dObj.size.y / 2f));

        int i = 0;
        foreach (string cT in dObj.collisionTypes)
        {
            foreach (DynamicObject other in checkBoxDynamic(cT, s, p))
            {
                ret[i] = other;
                i++;
            }
        }
        return ret;
    }

    private void ResetDynamicObjects()
    {
        //
    }

    private void pixelMarch(Vector2 mov)            //Moves the player pixel by pixel toward vel, stopping on collision. Returns remainder of vel.
    {
        float movX = mov.x;
        float movY = mov.y;

        float targetX = transform.position.x;
        float targetY = transform.position.y;

        while (Mathf.Abs(movX) > upp / 2 || Mathf.Abs(movY) > upp / 2)
        {
            if (Mathf.Abs(movX) >= Mathf.Abs(movY))
            {
                if (movX < 0)
                {
                    if (checkX(targetX - borderX))
                    {
                        movX += upp;
                        targetX -= upp;
                    }
                    else
                    {
                        //remVelX = movX;
                        movX = 0;
                    }
                }
                else
                {
                    if (checkX(targetX + borderX))
                    {
                        movX -= upp;
                        targetX += upp;
                    }
                    else
                    {
                        //remVelX = movX;
                        movX = 0;
                    }
                }
            }
            else
            {
                if (movY < 0)
                {
                    if (checkY(targetY - borderY))
                    {
                        movY += upp;
                        targetY -= upp;
                    }
                    else
                    {
                        //remVelY = movY;
                        movY = 0;
                    }
                }
                else
                {
                    if (checkY(targetY + borderY))
                    {
                        movY -= upp;
                        targetY += upp;
                    }
                    else
                    {
                        //remVelY = movY;
                        movY = 0;
                    }
                }
            }
        }

        target = new Vector2(targetX, targetY);
        transform.position = target;

        movement = new Vector2(movX, movY);
    }

    private Vector2 pixelClamp(Vector2 v)
    {
        Vector2 pixelVector = new Vector2(Mathf.RoundToInt(v.x * ppu), Mathf.RoundToInt(v.y * ppu));
        return pixelVector / ppu;
    }

}
