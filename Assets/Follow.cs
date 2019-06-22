using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follow : MonoBehaviour
{
    private GameObject follow;
    public Vector3 offset;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (follow == null) {
            Player player = Component.FindObjectOfType<Player>();
            if (player) {
                follow = player.gameObject;
            } else {
                Debug.LogError("missing player");
            }

        } else {
            this.transform.position = follow.transform.position + offset;
        }
    }
}
