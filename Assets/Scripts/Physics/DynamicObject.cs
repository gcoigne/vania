using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicObject : MonoBehaviour
{
    public PixelPhysics phys;

    public int unitToPixel;
    private float pixelToUnit;

    public int id;
    public string type;

    public List<string> overlapTypes;
    public List<string> collisionTypes;

    public float mass = 1;
    public Vector2 size = new Vector2(1, 1);
    public Vector2 pos;
    public Vector2 offset = Vector2.zero;
    public Vector2 vel = Vector2.zero;
    public Vector2 delta = Vector2.zero;
    public Vector2 grav = new Vector2(0, -10);
    public float maxSpeedX = 10f;
    public float maxSpeedY = 5f;

    public Vector2 projectedPos;
    public Vector2 tempPos;
    public List<StaticObject> nearbyStatic;
    public List<StaticObject> nearbyDynamic;
    public List<Overlap> overlaps = new List<Overlap>();
    public List<Collision> collisions = new List<Collision>();

    public float friction = 0;
    public float elasticity = 0;

    public class Box
    {
        public PixelPhysics phys;
        public Vector2 size;
        public Vector2 offset;

        public Vector2 bL;
        public Vector2 tR;

        public Box(PixelPhysics p, Vector2 s, Vector2 o)
        {
            phys = p;
            size = s;
            offset = o;

            bL = new Vector2(o.x - (s.x / 2f), o.y - (s.y / 2f));
            tR = new Vector2(o.x + (s.x / 2f), o.y + (s.y / 2f));
        }
    }

    public class Overlap
    {
        public int id;
        public bool dynamic;
        public string type;

        public Overlap(int i, bool d, string t)
        {
            id = i;
            dynamic = d;
            type = t;
        }
    }

    public class Collision
    {
        public bool dynamic;
        public int id;
        public string type;
        public Vector2 pos;
        public float angle;

        public Collision(bool d, int i, string t, Vector2 p, float a)
        {
            dynamic = d;
            id = i;
            type = t;
            pos = p;
            angle = a;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        pixelToUnit = 1f / unitToPixel;
        pos = pixelClamp((Vector2) transform.position + offset);
        pos = (Vector2) transform.position + offset;
        delta = (Vector2)transform.position - pos;
        id = phys.registerDynamic(this);
    }

    // Update is called once per frame
    void Update()
    {
    }    

    void LateUpdate()
    {
        //transform.position = pos - offset;
        //vel += grav * Time.deltaTime;
        //Vector2.ClampMagnitude(vel, maxSpeed);
        //delta += vel * Time.deltaTime;
        //collisions = new List<Collision>();
    }
    
    public void moveX(float distance)
    {
        pos.x += distance;
        delta.Set(delta.x - distance, delta.y);
    }
    
    public void moveY(float distance)
    {
        pos.y += distance;
        delta.Set(delta.x, delta.y - distance);
    }

    public void applyForce(Vector2 force)
    {
        vel += force / mass;
    }

    public int registerOverlap(int i, bool d, string t)
    {
        Overlap ov = new Overlap(i, d, t);
        overlaps.Add(ov);
        return overlaps.Count;
    }

    public void registerCol(bool d, int i, string t, Vector2 p, float a)
    {
        Collision col = new Collision(d, i, t, p, a);
        collisions.Add(col);
    }

    public bool matchingCol(bool d, float a, string t = "any")
    {
        foreach (Collision col in collisions)
        {
            if (col.dynamic == d && col.angle == a && (col.type == t || t == "any"))
            {
                return true;
            }
        }
        return false;
    }

    private Vector2 pixelClamp(Vector2 v)
    {
        Vector2 pixelVector = new Vector2(Mathf.RoundToInt(v.x * unitToPixel), Mathf.RoundToInt(v.y * unitToPixel));
        return pixelVector * pixelToUnit;
    }
}