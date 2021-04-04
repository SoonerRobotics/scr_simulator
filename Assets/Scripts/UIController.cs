using RosSharp.RosBridgeClient;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    private bool isPaused = false;
    public GameObject pauseUI;

    public void Pause()
    {
        if (!isPaused)
        {
            isPaused = true;
            pauseUI.SetActive(true);
        }
        else
        {
            isPaused = false;
            pauseUI.SetActive(false);
        }
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Menu()
    {
        SceneManager.LoadScene(0);
    }
    public void Quit()
    {
        Application.Quit();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            Pause();
        }

        if (!MenuController.runningWithoutROS && RosConnector.instance.IsConnected.WaitOne(0) == false)
        {
            SceneManager.LoadScene(0);
        }
    }
}
