using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneWorker : MonoBehaviour
{
    public KeyCode _restartKey;

    void Start()
    {
        
    }

    void Update()
    {
        SceneReload();
    }
    void SceneReload()
    {
        if (Input.GetKeyDown(_restartKey))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

}
