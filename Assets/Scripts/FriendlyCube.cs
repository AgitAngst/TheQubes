using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FriendlyCube : GameManager {
    // Start is called before the first frame update
    void Start () {
        gameObject.tag = "Friend";
    }

    // Update is called once per frame
    void Update () {
        MoveCube ();
    }

    private void OnTriggerEnter (Collider other) {
        if (other.CompareTag ("Finish")) {
            GetComponent<Rigidbody> ().isKinematic = false;
            Destroy (gameObject, 1.5f);
        }
    }
}