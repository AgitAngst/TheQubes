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
            Instantiate(gameManager.locatorToSpawn, new Vector3(hit.collider.gameObject.transform.position.x, hit.collider.gameObject.transform.position.y + 1, hit.collider.gameObject.transform.position.z), Quaternion.identity);
            Debug.Log(hit.collider.name);
        }
    }
}