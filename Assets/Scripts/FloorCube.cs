using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorCube : Cube
{
    // Start is called before the first frame update
    void Start () {
        gameObject.tag = "Floor";
    }

    // Update is called once per frame
    void Update () {
    }
}