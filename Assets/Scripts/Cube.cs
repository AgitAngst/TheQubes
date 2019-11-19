using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Cube : MonoBehaviour {
    GameManager gameManager;
    void Start () {
        gameManager = GameObject.Find ("GameManager").GetComponent<GameManager> ();
    }

    void Update () {

    }
    public void N () {
        gameManager.MoveCube ();
    }

}