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

    public float girdSpacingOffset = 1f;
    public int greenCubeNum = 5;

    public GameObject cubeToSpawnRed;
    public GameObject cubeToSpawnGreen;
    public GameObject cubeToSpawnBlack;

    public GameObject floorCube;

    GameObject tmpSpawn;
    Vector3 pos;
    public Vector3 grdOrigin = Vector3.zero;
   

    void Start () {
        timerCounter = timer;
        pos = transform.position;
        GenerateCubes ();
    }
    private void Update () {
        if (Input.GetKeyDown (KeyCode.Space)) {
            GenerateCubes ();
        }
        MoveCube ();
    }
    public void MoveCube () {
        timerCounter -= Time.deltaTime;
        if (timerCounter <= 0f) {
            MoveAndRotate ();
            timerCounter = timer;
        }
    }
    public void MoveAndRotate () {
        transform.DORotate (new Vector3 (90, 0, 0), animationSpeed, RotateMode.LocalAxisAdd);
        var tmp = transform.position.z + 1;
        transform.DOLocalMoveZ (tmp, animationSpeed);
    }
    public void GenerateCubes () {
        GenerateEnemyCubes ();
        GenerateFloorCubes ();
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
        for (int x = 0; x < cubesCountX; x++) {
            for (int z = 0; z < cubesFloorCountZ; z++) {
                Vector3 spawnPosition = new Vector3 (x * girdSpacingOffset, -1, z * girdSpacingOffset) + grdOrigin;
                SpawnFloorCubes (spawnPosition, Quaternion.identity);
            }
        }
    }

    void SpawnCubes (Vector3 positionToSpawn, Quaternion rotationToSpawn, CubeColor color) {
        GameObject clone;

        switch (color) {
            case CubeColor.Red:
                clone = Instantiate (cubeToSpawnRed, positionToSpawn, rotationToSpawn);
                break;

            case CubeColor.Green:
                clone = Instantiate (cubeToSpawnGreen, positionToSpawn, rotationToSpawn);
                break;

            case CubeColor.Black:
                clone = Instantiate (cubeToSpawnBlack, positionToSpawn, rotationToSpawn);
                break;
        }
    }
    void SpawnFloorCubes (Vector3 positionToSpawn, Quaternion rotationToSpawn) {
        GameObject clone = Instantiate (floorCube, positionToSpawn, rotationToSpawn);

    }
}