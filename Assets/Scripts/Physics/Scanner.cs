using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scanner : MonoBehaviour
{
    public PixelPhysics phys;

    public Vector2 size = new Vector2(1f, 1f);
    public Vector2 offset;
    public Vector2 pos;
    private Vector2 bL;
    private Vector2 tR;
    void Start()
    {
    }
    void Update()
    {
        
    }

    private void setPosition()
    {
        pos = (Vector2) transform.position + offset;
        bL = pos - size / 2f;
        tR = pos + size / 2f;
    }

    public List<StaticObject> checkStatic(string t = "any")
    {
        setPosition();
        List<StaticObject> stObjs;
        if (t == "any")
        {
            stObjs = phys.staticObjects;
        }
        else
        {
            stObjs = phys.staticObjects.FindAll(x => x.type == t);
        }
        return phys.checkBoxStatic(bL, tR, stObjs);
    }
    public List<DynamicObject> checkDynamic(string t = "any")
    {
        setPosition();
        List<DynamicObject> dynObjs;
        if (t == "any")
        {
            dynObjs = phys.dynamicObjects;
        }
        else
        {
            dynObjs = phys.dynamicObjects.FindAll(x => x.type == t);
        }
        return phys.checkBoxDynamic(bL, tR, dynObjs);
    }
}
