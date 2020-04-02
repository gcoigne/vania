using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Creator : MonoBehaviour {

    // Assets
    public Transform playerT;
    public Transform platformT;
    public Transform onewayT;

    public TextAsset levelText;

	// Use this for initialization
	void Start ()
    {
        //createPlayer(new Vector3(0f, 0f, 0f));
        createPlatform(new Vector3(0f, -3.5f, 4), 128f);
        createPlatform(new Vector3(-5f, 0.5f, 0f), 4f);
        createOneway(new Vector3(5f, 0.5f, 0f), 4f);
	}
	
	// Update is called once per frame
	void Update ()
    {

	}

    // Creates objects based on the provided text file.
    private void createLevel(string levelCode)
    {

    }

    private void ProcessLevelText(TextAsset levelText)
    {
        var lineSplit = levelText.text.Split(new[] { '\r', '\n' });
        foreach (string line in lineSplit)
        {
            var commaSplit = line.Split(',');
            if (commaSplit.Length > 0)
                addTextElement(commaSplit);
        }
    }
    
    private void addTextElement(string[] description)
    {

    }

    private void createPlayer (Vector3 pos)
    {
        Transform newPlayerT = Transform.Instantiate(playerT, pos, Quaternion.identity);
        newPlayerT.gameObject.name = "Player";
    }

    // Platform is divided into two equal vertical slices.
    private void createPlatform (Vector3 pos, float width, float height=1f)
    {
        Transform newPlatformT = Transform.Instantiate(platformT, pos, Quaternion.identity);
        newPlatformT.localScale = new Vector3(width, height, 1f);
        Transform floorLayerT = newPlatformT.transform.Find("Floor");
        Transform ceilingLayerT = newPlatformT.transform.Find("Ceiling");
    }

    //Oneway only registers collisions through its top face.
    private void createOneway (Vector3 pos, float width, float height=1f)
    {
        Transform newOnewayT = Transform.Instantiate(onewayT, pos, Quaternion.identity);
        newOnewayT.localScale = new Vector3(width, height, 1f);
        Transform floorLayerT = newOnewayT.transform.Find("Floor");
    }

}
