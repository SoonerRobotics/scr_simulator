using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPSPrinter : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Transform[] xs = GetComponentsInChildren<Transform>();
        foreach (Transform x in xs)
        {
            if (x.parent == transform) {
                Vector3 pos = x.position;
                double lat = pos.z / 110944.12 + 35.194881f;
                double lon = pos.x / 91071.17 + -97.438621f;

                Debug.Log(lat + "," + lon);
            }
        }
    }
}
