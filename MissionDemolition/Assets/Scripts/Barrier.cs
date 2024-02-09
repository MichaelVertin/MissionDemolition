using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class Barrier : MonoBehaviour
{
    private Vector3 origPos;
    private Vector3 velocity = new Vector3(0, 4, 0);
    float turnAround = 10f;

    // Start is called before the first frame update
    void Start()
    {
        origPos = transform.position;
        this.GetComponent<Rigidbody>().velocity = velocity;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if( (transform.position - origPos ).magnitude > turnAround )
        {
            origPos = transform.position;
            velocity *= -1;
            this.GetComponent<Rigidbody>().velocity = velocity;
        }
    }
}
