using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Creator : MonoBehaviour {

    // References
    public Transform floorSquare;
    public Transform ceilingSquare;

    public float envDepth;

	// Use this for initialization
	void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    public void createLevel (string levelCode)
    {

    }

    private void createPlatform (Vector3 pos, float width, float height=1f, float rz=0f)
    {
        Transform.Instantiate(floorSquare, new Vector3)
    }

    private void createOneWay (Vector3 pos, float width, float height=.25f, float rz=0f)
    {

    }
}
