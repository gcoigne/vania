using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Conversation : MonoBehaviour
{
    public GameObject dialogManager;
    public string dialogName;

    private DialogManagement dialogManagement;

    // Start is called before the first frame update
    void Start()
    {
        dialogManagement = dialogManager.GetComponent<DialogManagement>();
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
            dialogManagement.startDialog(dialogName);
        }
    }
}
