using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{

    public float speed = 0.1f;

    private void Start()
    {
        // Unless we want a reloading config, we just need to pull this on start
        // However, if we want a reloading config, an event handler could be set up so that when it updates we could change it here
        speed = ConfigLoader.Instance.configExample.cameraSpeed; 
    }

    void OnGUI()
    {
        this.transform.Translate(Input.mouseScrollDelta.y * this.transform.forward * speed);
    }
}
