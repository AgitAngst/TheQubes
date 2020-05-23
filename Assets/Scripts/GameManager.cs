using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;



public enum CubeColor {
    Red = 0,
    Green = 1,
    Black = 2
}

public class GameManager : MonoBehaviour {
    //public static GameManager instance;
    public float timer = 3f;
    [HideInInspector] public float timerCounter;
    public float animationSpeed = 0.2f;

    public int cubesCountX = 10;
    public int cubesCountZ = 10;
    public int cubesFloorCountZ = 15;
    public float ggridSpacingOffsetFloor = 1f;
    public float girdSpacingOffset = 1f;
    public int greenCubeNum = 5;

    public GameObject cubeToSpawnRed;
    public GameObject cubeToSpawnGreen;
    public GameObject cubeToSpawnBlack;
    public GameObject locatorToSpawn;


    public GameObject floorCube;

    public List<GameObject> gameObjectsArr = new List<GameObject>();
    public List<GameObject> floorArr = new List<GameObject>();


    int cloneCounter = 0;
    GameObject tmpSpawn;
    Vector3 pos;
    public Vector3 grdOrigin = Vector3.zero;
   

    void Start () {
        timerCounter = timer;
        pos = transform.position;
        GenerateFloorCubes();

    }
    private void Update () {
        if (Input.GetKeyDown (KeyCode.Space)) {
            GenerateEnemyCubes();
        }
    }
    public void MoveCube (Transform cube) {
        timerCounter -= Time.deltaTime;
        if (timerCounter <= 0f) {
            MoveAndRotate (cube);
            timerCounter = timer;
        }
    }
    public void MoveAndRotate (Transform cube) {
        cube.DORotate (new Vector3 (90, 0, 0), animationSpeed, RotateMode.LocalAxisAdd);
        var tmp = cube.position.z + 1;
        cube.DOLocalMoveZ (tmp, animationSpeed);
    }
   

    void GenerateEnemyCubes () {
        bool spawnedThisLine = false;

        for (int x = 0; x < cubesCountX; x++) {
            for (int z = 0; z < cubesCountZ; z++) {
                Vector3 spawnPosition = new Vector3 (x * girdSpacingOffset, 0, z * girdSpacingOffset) + grdOrigin;
                bool needSpawnGreen = Random.Range (0f, 1f) > 0.85f && greenCubeNum > 0;

                if (needSpawnGreen) {
                    greenCubeNum--;
                    spawnedThisLine = true;
                }

                SpawnCubes (spawnPosition, Quaternion.identity, needSpawnGreen? CubeColor.Green : CubeColor.Red);
            }
            spawnedThisLine = false;
        }
    }

    void GenerateFloorCubes () {
        GameObject floor = new GameObject("Floor");
        for (int x = 0; x < cubesCountX; x++) {
            for (int z = 0; z < cubesFloorCountZ; z++) {
                Vector3 spawnPosition = new Vector3 (x * ggridSpacingOffsetFloor, -1, z * ggridSpacingOffsetFloor) + grdOrigin;
                GameObject clone = Instantiate(floorCube, spawnPosition, Quaternion.identity);
                floorArr.Add(clone);
            }
        }

        foreach (var item in floorArr)
        {
            item.transform.parent = floor.transform;

        }
    }

    void SpawnCubes (Vector3 positionToSpawn, Quaternion rotationToSpawn, CubeColor color) {
        GameObject clone;
        int count = 0;
        switch (color) {
            case CubeColor.Red:
                GameObject newGroup = new GameObject("TestGroup");

                if (count < 4)
                {
                    count++;
                    gameObjectsArr.Add(Instantiate(cubeToSpawnRed, positionToSpawn, rotationToSpawn));
                    
                }
                foreach (GameObject item in gameObjectsArr)
                {
                    item.transform.parent = newGroup.transform;
                }
                
                break;

            case CubeColor.Green:
                clone = Instantiate (cubeToSpawnGreen, positionToSpawn, rotationToSpawn);
                break;

            case CubeColor.Black:
                clone = Instantiate (cubeToSpawnBlack, positionToSpawn, rotationToSpawn);
                break;
        }
    }


    
}