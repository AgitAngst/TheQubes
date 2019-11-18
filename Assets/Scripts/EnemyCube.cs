using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCube : CubeMoveAndRotate
{
    // Start is called before the first frame update
    void Start()
    {
        gameObject.tag = "Enemy";
    }

    // Update is called once per frame
    void Update()
    {
        Timer();
    }
}
