using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Cube : MonoBehaviour {
    GameManager gameManager;
    void Awake () {
        gameManager = FindObjectOfType<GameManager>();
    }

    void Update () {

    }
    public void Move(Transform cube) {
        gameManager.MoveCube (cube);
    }

}