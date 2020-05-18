using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        
            Debug.DrawRay(new Vector3(transform.position.x - 0.5f, transform.position.y, transform.position.z), new Vector3(-0.5f, 0, 0), Color.green, 5f);
            Debug.DrawRay(new Vector3(transform.position.x + 0.5f, transform.position.y, transform.position.z), new Vector3(0.5f, 0, 0), Color.green, 5f);
            Debug.DrawRay(new Vector3(transform.position.x, transform.position.y, transform.position.z + 0.5f), new Vector3(0, 0, 0.5f), Color.green, 5f);
            Debug.DrawRay(new Vector3(transform.position.x, transform.position.y, transform.position.z - 0.5f), new Vector3(0, 0, -0.5f), Color.green, 5f);

    }
}
