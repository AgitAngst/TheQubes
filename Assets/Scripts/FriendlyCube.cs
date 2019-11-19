using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FriendlyCube : CubeManager
{
    // Start is called before the first frame update
    void Start () {
        gameObject.tag = "Friend";
    }

    // Update is called once per frame
    void Update () {
        MoveCube ();
    }
}