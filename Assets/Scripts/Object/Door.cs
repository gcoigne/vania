using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Door : MonoBehaviour
{
    public string sceneName;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        GameObject c = collision.gameObject;
        if (c.tag == "Player" && Input.GetButtonDown("Interact"))
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
