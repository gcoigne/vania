using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dart : MonoBehaviour
{
    public float angle = 0f;
    public float speed = 1f;
    public int lifetime = 60;

    private Vector3 vel;
    private int timer;

    // Start is called before the first frame update
    void Start()
    {
        vel = new Vector3(speed * Mathf.Cos(angle * Mathf.Deg2Rad), speed * Mathf.Sin(angle * Mathf.Deg2Rad), 0f);

        timer = Time.frameCount;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        if (Time.frameCount > timer + lifetime)
        {
            Destroy(gameObject);
        }
        transform.position += vel;
    }
}
