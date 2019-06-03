using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Attached to Player's melee attack objects.
// Set attackData when you instantiate it using its associated text file. 
// Deletes self once it has completed all frames.
public class Player_Melee : MonoBehaviour
{
    public string attackData;               // Information about the attack. Should be read from a text file in Resources.
    private string[] frameData;             // Data about each frame of the attack
    private int frameCounter = 0;           // When frameCounter exceeds length of frameData, delete self.
    private GameObject[] hitboxes;          // Each hitbox that is active on the current frame. Hitboxes are deleted at end of frame.
    private GameObject[] hits;              // Each valid GameObject the attack has already hit.

	// Use this for initialization
	void Start ()
    {
        frameData = attackData.Split('\n');
	}
	
	// Update is called once per frame
	void Update ()
    {

	}

    private void FixedUpdate()
    {
        #region Generate hitboxes
        string frame = frameData[frameCounter];
        string[] hitboxData = frame.Split(';');
        foreach (string hbd in hitboxData) 
        {
            string[] arguments = hbd.Split(',');

        }
        #endregion

        #region Register hits
        #endregion

        #region Cleanup
        foreach (GameObject hitbox in hitboxes)
        {
            Destroy(hitbox);
        }
        frameCounter++;
        #endregion
    }
}
