using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class CubeMoveAndRotate : MonoBehaviour {

    public float timer = 3f;
    float timerCounter;
    public float animationSpeed = 0.2f;

    void Start () {
        timerCounter = timer;
    }

    void Update () {
        if (Input.GetKeyDown (KeyCode.Space)) {
            MoveAndRotate ();
        }
        Timer();
    }

   public void MoveAndRotate () {
        transform.DORotate (new Vector3 (90, 0, 0), animationSpeed, RotateMode.LocalAxisAdd);
        var tmp = transform.position.z + 1;
        transform.DOLocalMoveZ (tmp, animationSpeed);
    }

    public void Timer () {
        timerCounter -= Time.deltaTime;
        if (timerCounter <= 0f) {
            MoveAndRotate ();
            timerCounter = timer;
        }
    }
}