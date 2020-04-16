using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticObject : MonoBehaviour
{
    public PixelPhysics physicsController;

    public int unitToPixel;
    private float pixelToUnit;

    public int id;
    public string type;

    public Vector2 size = new Vector2(1,1);
    public Vector2 pos;
    public Vector2 offset;

    public float friction;
    public float elasticity;

    // Start is called before the first frame update
    void Start()
    {
        pos = pixelClamp((Vector2)transform.position + offset);
        id = physicsController.registerStatic(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private Vector2 pixelClamp(Vector2 v)
    {
        Vector2 pixelVector = new Vector2(Mathf.RoundToInt(v.x * unitToPixel), Mathf.RoundToInt(v.y * unitToPixel));
        return pixelVector / unitToPixel;
    }
}
