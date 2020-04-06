using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dart : MonoBehaviour
{
    public float angle = 0f;
    public float speed = 1f;
    public int lifetime = 60;

    private Rigidbody2D rb;

    private Vector3 vel;
    private int timer;
    private bool timerActive;

    private LayerMask roughMask;
    private LayerMask smoothMask;

    private Collider2D col;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        vel = new Vector3(speed * Mathf.Cos(angle * Mathf.Deg2Rad), speed * Mathf.Sin(angle * Mathf.Deg2Rad), 0f);
        rb.velocity = vel;

        timer = Time.frameCount;
        timerActive = true;

        col = GetComponent<Collider2D>();

        smoothMask = LayerMask.GetMask("Smooth");
        roughMask = LayerMask.GetMask("Rough");

        rb.Sleep();
    }

    // Update is called once per frame
    void Update()
    {
        if (timerActive && Time.frameCount > timer + lifetime)
        {
            Destroy(gameObject);
        }
    }

    private void FixedUpdate()
    {
        transform.position += vel * Time.deltaTime;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Collider2D otherCol = collision.collider;
        GameObject other = otherCol.gameObject;
        LayerMask otherMask = other.layer;
        Debug.Log(other.layer.ToString());
        if (other.layer.ToString() == "10")
        {
            rb.Sleep();
            timerActive = false;
        }
    }
}
