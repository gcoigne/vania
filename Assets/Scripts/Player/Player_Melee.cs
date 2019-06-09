using System;
using System.Collections.Generic;
using UnityEngine;

// Attached to Player's melee attack objects.
// Set attackData when you instantiate it using its associated text file. 
// Deletes self once it has completed all frames.
public class Player_Melee : MonoBehaviour
{
    public GameObject hitboxRect;               // Prefab for a rectangular hitbox

    public string attackData;                   // Information about the attack. Should be read from a text file in Resources.
    private string[] frameData;                 // Data about each frame of the attack

    private int frameTotal = 0;                 // Total number of frames in the attack.
    private int frameCounter = 0;               // When frameCounter exceeds length of frameData, delete self.

    private List<GameObject> hitboxes;          // Each hitbox that is active on the current frame. Hitboxes are deleted at end of frame.
    private List<float> damageValues;           // The damage each current hitbox will do.
    private List<Vector2> knockBackVectors;     // The force each current hitbox will apply.
    private List<GameObject> hits;              // Each valid GameObject the attack has already hit.
    private int maxHits = 16;                   // Max colliders each hitbox can register a collision with.

    private ContactFilter2D enemiesFilter;      // Contact filter that keeps enemies.

	// Use this for initialization
	void Start ()
    {
        frameData = attackData.Split('\n');
        frameTotal = frameData.Length;

        ContactFilter2D enemiesFilter = new ContactFilter2D();
        enemiesFilter.SetLayerMask(LayerMask.GetMask("Enemies"));
	}
	
	// Update is called once per frame
	void Update ()
    {

	}

    private void FixedUpdate()
    {
        #region Out of frames
        if (frameCounter >= frameTotal)
        {
            Destroy(gameObject);
        }
        #endregion

        else
        {
            #region Generate hitboxes
            string frame = frameData[frameCounter];
            string[] hitboxData = frame.Split(';');

            foreach (string hbd in hitboxData)
            {
                if (hbd == "clear")
                {
                    // Clears hit registration, allowing for another hit per target. Does not generate a hitbox.
                    hits.Clear();
                    continue;
                }

                string[] hbArgs = hbd.Split(',');
                if (hbArgs.Length < 6)
                {
                    print("Incorrect number of hitbox arguments");
                    continue;
                }

                string hbType = hbArgs[0];
                Vector2 hbPos = new Vector2(float.Parse(hbArgs[1]), float.Parse(hbArgs[2]));
                Quaternion hbRot = Quaternion.Euler(0f, 0f, float.Parse(hbArgs[3]));
                float hbXScale = float.Parse(hbArgs[4]);
                float hbYScale = float.Parse(hbArgs[5]);
                float hbDamage = float.Parse(hbArgs[6]);
                Vector2 HbKnockback = new Vector2(float.Parse(hbArgs[7]), float.Parse(hbArgs[8]));

                switch (hbArgs[0])
                {
                    case "e":
                        //TODO import ellipse colliders
                        break;

                    case "t":
                        //TODO implement trianular hitboxes
                        break;

                    case "r":
                        GameObject hbNew = Instantiate(hitboxRect, hbPos, hbRot, transform);
                        Transform hbNewT = hbNew.transform;
                        hbNewT.localScale = new Vector2(hbXScale, hbYScale);
                        hitboxes.Add(hbNew);
                        break;
                }
            }
            #endregion

            #region Register hits
            for (int hbI = 0; hbI > hitboxes.Count; hbI++) 
            {
                Collider2D hbCol = hitboxes[hbI].GetComponent<Collider2D>();
                Collider2D[] hbHitsA = new Collider2D[maxHits];
                List<Collider2D> hbHits = new List<Collider2D>(Physics2D.OverlapCollider(hbCol, enemiesFilter, hbHitsA));
                foreach (Collider2D hitCol in hbHits)
                {
                    GameObject hit = hitCol.gameObject;
                    if (!hits.Contains(hit))
                    {
                        hit.SendMessage("applyDamage", damageValues[hbI]);
                        hit.SendMessage("applyKnockback", knockBackVectors[hbI]);
                        hits.Add(hit);
                    }
                }
            }
            #endregion

            #region Cleanup
            if (hitboxes != null)
            {
                foreach (GameObject hitbox in hitboxes)
                {
                    Destroy(hitbox);
                }
                Array.Clear(hitboxes, 0, hitboxes.Length);
            }
            frameCounter++;
            #endregion
        }

    }
}
