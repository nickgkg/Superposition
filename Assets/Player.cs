using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Start is called before the first frame update
    public Rigidbody body;
    public Collider collider;
    public float speed;

    // Update is called once per frame
    void Update()
    {
        if (collisonCount > 0) {
            if (Input.GetKey(KeyCode.UpArrow)) {
                body.AddForce(new Vector3(0, 0, speed));
            }
            if (Input.GetKey(KeyCode.LeftArrow)) {
                body.AddForce(new Vector3(-speed, 0, 0));
            }
            if (Input.GetKey(KeyCode.RightArrow)) {
                body.AddForce(new Vector3(speed, 0, -0));
            }
            if (Input.GetKey(KeyCode.DownArrow)) {
                body.AddForce(new Vector3(0, 0, -speed));
            }
        }
    }

    int collisonCount = 0;
    void OnCollisionEnter(Collision other) {
        collisonCount++;
    }
    void OnCollisionExit(Collision other) {
        collisonCount--;
    }

}
