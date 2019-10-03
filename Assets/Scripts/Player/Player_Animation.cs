using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Animation : MonoBehaviour {
    public GameObject hitboxRect;               // Prefab for a rectangular hitbox

    public string attackData;                   // Information about the attack. Should be read from a text file in Resources.
    private string[] frameData;                 // Data about each frame of the attack

    private int frameTotal = 0;                 // Total number of frames in the current animation.
    private int frameCounter = 0;               // When frameCounter exceeds length of frameData, end animation.

    private ContactFilter2D enemiesFilter;      // Contact filter that keeps enemies.

    // Rectangle with center at Pos relative to player coordinates. Used for checking incoming attacks.
    class Hurtbox
    {
        public Vector2 Pos, Size;
        public string Type;
        public Hurtbox(float x, float y, float width, float height, string type)
        {
            Pos = new Vector2(x, y);
            Size = new Vector2(width, height);
            Type = type;
        }
    }

    // Rectangle with center at Pos relative to player coordinates. Used for checking outgoing attacks.
    class Hitbox
    {
        public Vector2 Pos, Size;
        public string Type;
        public ContactFilter2D Filter;
        public Collider2D[] Hits;
        public Hitbox(float x, float y, float width, float height, string type, ContactFilter2D filter)
        {
            Pos = new Vector2(x, y);
            Size = new Vector2(width, height);
            Type = type;
            Filter = filter;
        }

        // Updates Hits, returns number of hits.
        public int CheckHits()
        {
            int colNum = Physics2D.OverlapBoxNonAlloc(Pos, Size, 0f, Hits);
            return colNum;
        }
    }

    // Use this for initialization
    void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    private void FixedUpdate()
    {
        
    }
}
