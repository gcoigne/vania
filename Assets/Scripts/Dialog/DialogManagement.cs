using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogManagement : MonoBehaviour
{
    public class DialogLine
    {
        public char speaker;            //Speaker "a", "b" or "c" (Third party)
        public string text;             //Value of textbox
        public string eventName;        //Name of event to be called
        public DialogLine next;         //Next line
        public DialogLine response0;    //Reserved for Leave dialog
        public DialogLine response1;    //First response option
        public DialogLine response2;    //Second
        public DialogLine response3;    //Third

        public DialogLine(char s, string t)
        {
            speaker = s;
            text = t;
        }
    }

    public GameObject eventManager;

    private Canvas canvas;
    private UnityEngine.UI.Text text;
    private GameObject portraitA;
    private GameObject portraitB;
    private DialogLine root;
    private DialogLine exit;
    private EventManagement eventManagement;

    // Start is called before the first frame update
    private void Start()
    {
        canvas = transform.Find("Canvas").gameObject.GetComponent<Canvas>();
        portraitA = transform.Find("Canvas/PortraitA").gameObject;
        portraitB = transform.Find("Canvas/PortraitB").gameObject;
        text = transform.Find("Canvas/Text").gameObject.GetComponent<UnityEngine.UI.Text>();
        eventManagement = eventManager.GetComponent<EventManagement>();
        root = new DialogLine('b', "Welcome to the pre-pre-pre-pre-pre-\npre-pre-pre-pre-pre-pre-pre-pre-pre-\npre-pre-pre-pre-pre-pre-pre-pre-pre-\npre-pre-pre-pre-pre-pre-pre-alpha");
        exit = new DialogLine('b', "See ya");
        exit.eventName = "exit";
        root.response0 = exit;
    }

    // Update is called once per frame
    private void Update()
    {
        
    }

    private DialogLine generateLines(string[] strings, DialogLine exitLine, DialogLine conclusion)
    {
        int l = strings.Length;
        if (l < 1)
        {
            return conclusion;
        }

        char s = strings[0][0];
        string t = strings[0].Substring(1);

        DialogLine root = new DialogLine(s, t);
        root.response0 = exitLine;
        DialogLine cur = root;
        DialogLine tmp;

        for (int i = 1; i < l; i++)
        {
            s = strings[i][0];
            if (strings[i].Length > 0)
                t = strings[i].Substring(1);
            else
                t = "";

            tmp = new DialogLine(s, t);
            tmp.response0 = exitLine;
            cur.response1 = tmp;
            cur = tmp;
        }

        cur.response1 = conclusion;

        return root;
    }
    
    private void display(string value)
    {
        text.text = value;
        //Make options interactable once all text is displayed
    }

    public void startDialog(string name)
    {
        canvas.gameObject.SetActive(true);
        dialog(root);
    }

    private void dialog(DialogLine root)
    {
        display(root.text);

        if (root.eventName != null)
        {
            //Call event by name
        }
    }
}
