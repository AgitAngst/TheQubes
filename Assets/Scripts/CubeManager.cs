using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class CubeManager : MonoBehaviour {

    public float timer = 3f;
    float timerCounter;
    public float animationSpeed = 0.2f;

    public int cubesCountX = 10;
    public int cubesCountZ = 10;
    public float girdSpacingOffset = 1f;

    public GameObject cubeToSpawn;
    GameObject tmpSpawn;
    Vector3 pos;
    public Vector3 grdOrigin = Vector3.zero;

    void Start () {
        timerCounter = timer;
        pos = transform.position;
        GenerateCubes ();
    }

    void Update () {
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

        for (int x = 0; x < cubesCountX; x++) {
            for (int z = 0; z < cubesCountZ; z++) {
                Vector3 spawnPosition = new Vector3 (x * girdSpacingOffset, 1, z * girdSpacingOffset) + grdOrigin;
                SpawnCubes(spawnPosition,Quaternion.identity);
            }
        }
    }

    void SpawnCubes(Vector3 positionToSpawn, Quaternion rotationToSpawn)
    {
        GameObject clone = Instantiate(cubeToSpawn, positionToSpawn,rotationToSpawn);
    }
}