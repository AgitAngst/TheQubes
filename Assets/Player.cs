using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Start is called before the first frame update

    public Transform spawnPosition;
    public GameObject player;
    GameObject playerSpawn;
    public Transform raycastShooter;
    int layerMask = 1 << 8;
    GameManager gameManager;
    GameObject cloned;
    int locatorSpawnCount = 0;
    void Start()
    {
        playerSpawn = Instantiate(player, spawnPosition);
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        raycastShooter = GameObject.Find("RaycastShooter").GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        SpawnLocator();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Floor"))
        {
        }
    }

    private void SpawnLocator()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            Raycast();
            Vector3 pos;
            pos = new Vector3(player.transform.position.x, player.transform.position.y + 1f, player.transform.position.z);
            //Instantiate(gameManager.locatorToSpawn, pos, Quaternion.identity);
        }
    }

    private static void CameraRayCast()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, 10000f))
        {
            Transform objectHit = hit.transform;
            Debug.Log(objectHit.name);
            // Do something with the object that was hit by the raycast.
        }
    }

    private void Raycast()
    {
        Vector3 start = raycastShooter.transform.position;
        Vector3 direction = new Vector3(start.x, -1, start.z);
        RaycastHit hit;
        if (Physics.Raycast(start, direction, out hit))
        {
            //hit.collider.gameObject.SetActive(false);

            if (locatorSpawnCount >= 1)
            {
                Destroy(cloned);
                locatorSpawnCount = 0;

            }
            cloned = Instantiate(gameManager.locatorToSpawn, new Vector3(hit.collider.gameObject.transform.position.x, hit.collider.gameObject.transform.position.y + 1, hit.collider.gameObject.transform.position.z), Quaternion.identity);

            Debug.Log(hit.collider.name);
            locatorSpawnCount++;

        }

        RaycastSideHit(new Vector3(-0.5f, 0, 0), new Vector3(-0.5f,0,0));
        RaycastSideHit(new Vector3(0.5f, 0, 0), new Vector3(0.5f, 0, 0));
        RaycastSideHit(new Vector3(0, 0, 0.5f), new Vector3(0, 0, 0.5f));
        RaycastSideHit(new Vector3(0, 0, -0.5f), new Vector3(0, 0, -0.5f));



    }

    private void RaycastSideHit(Vector3 side, Vector3 lenght)
    {
        RaycastHit hit;
        if (Physics.Raycast(new Vector3(cloned.transform.position.x + side.x, cloned.transform.position.y + side.y, cloned.transform.position.z + side.z), lenght, out hit))
        {
            Debug.DrawRay(new Vector3(cloned.transform.position.x + side.x, cloned.transform.position.y + side.y, cloned.transform.position.z + side.z), lenght, Color.green, 5f);
           
            if (hit.collider.gameObject.CompareTag("Enemy") || hit.collider.gameObject.CompareTag("Friend"))
            {
                hit.collider.gameObject.SetActive(false);
            }
        }
    }

    //Debug.DrawRay(new Vector3(transform.position.x - 0.5f, transform.position.y, transform.position.z), new Vector3(-0.5f, 0, 0), Color.green, 5f);
    //        Debug.DrawRay(new Vector3(transform.position.x + 0.5f, transform.position.y, transform.position.z), new Vector3(0.5f, 0, 0), Color.green, 5f);
    //      Debug.DrawRay(new Vector3(transform.position.x, transform.position.y, transform.position.z + 0.5f), new Vector3(0, 0, 0.5f), Color.green, 5f);
    //    Debug.DrawRay(new Vector3(transform.position.x, transform.position.y, transform.position.z - 0.5f), new Vector3(0, 0, -0.5f), Color.green, 5f);

    /* RaycastHit cubeSide2Hit;
        if (Physics.Raycast(new Vector3(cloned.transform.position.x - 0.5f, cloned.transform.position.y, cloned.transform.position.z), new Vector3(0.5f, 0, 0), out cubeSide2Hit))
        {
            Debug.DrawRay(new Vector3(cloned.transform.position.x - 0.5f, cloned.transform.position.y, cloned.transform.position.z), new Vector3(0.5f, 0, 0), Color.green, 5f);
            Debug.Log("Left:" + cubeSide2Hit.collider.name);
            cubeSide2Hit.collider.gameObject.SetActive(false);
        }

        RaycastHit cubeSide3Hit;
        if (Physics.Raycast(new Vector3(cloned.transform.position.x - 0.5f, cloned.transform.position.y, cloned.transform.position.z), new Vector3(0, 0, 0.5f), out cubeSide3Hit))
        {
            Debug.DrawRay(new Vector3(cloned.transform.position.x - 0.5f, cloned.transform.position.y, cloned.transform.position.z), new Vector3(0, 0, 0.5f), Color.green, 5f);
            Debug.Log("Left:" + cubeSide3Hit.collider.name);
            cubeSide3Hit.collider.gameObject.SetActive(false);
        }

        RaycastHit cubeSide4Hit;
        if (Physics.Raycast(new Vector3(cloned.transform.position.x - 0.5f, cloned.transform.position.y, cloned.transform.position.z), new Vector3(0, 0, -0.5f), out cubeSide4Hit))
        {
            Debug.DrawRay(new Vector3(cloned.transform.position.x - 0.5f, cloned.transform.position.y, cloned.transform.position.z), new Vector3(0, 0, -0.5f), Color.green, 5f);
            Debug.Log("Left:" + cubeSide4Hit.collider.name);
            cubeSide4Hit.collider.gameObject.SetActive(false);
        }*/
}