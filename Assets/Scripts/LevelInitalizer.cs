using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LevelInitalizer : MonoBehaviour
{

    public GameObject spawnLocation;
    public UnityEvent onRobotInstantiate;

    public static GameObject robot;

    // Start is called before the first frame update
    void Awake()
    {
        if (spawnLocation)
        {
            Vector3 pos = spawnLocation.transform.position;
            Quaternion rot = spawnLocation.transform.rotation;

            if (MenuController.activeRobot != null) {
                GameObject robotObj = Instantiate(MenuController.activeRobot.robotPrefab, pos, rot);
                robot = robotObj;
                onRobotInstantiate.Invoke();
            }

            Destroy(spawnLocation);
        }
    }
}
